using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Discussions.DbModel;
using Discussions.model;
using LoginEngine;

namespace Discussions
{
    public class LoginDriver
    {
        public static LoginResult Run(LoginFlow loginFlow)
        {
            switch (loginFlow)
            {
                case LoginFlow.ForEventGen:
                    return LoginFlowEventGen();
                case LoginFlow.ForExperiment:
                    return LoginFlowExperiment();
                case LoginFlow.Regular:
                    return LoginFlowRegular();
                default:
                    throw new NotSupportedException();
            }
        }

        private static LoginResult LoginFlowRegular()
        {
            SelectPerson:
            //person            
            var personDlg = new LoginPerson(false);
            personDlg.ShowDialog();

            if (personDlg.SelectedPerson == null)
                return null;

            //discussion
            var discussionDlg = new LoginDiscussionDlg(personDlg.SelectedPerson);
            discussionDlg.ShowDialog();

            if (discussionDlg.BackClicked)
                goto SelectPerson;

            if (discussionDlg.SelectedDiscussion == null)
            {
                MsgParticipantsShouldSelectDiscussion();
                return null;
            }

            var res = new LoginResult();

            //build result           
            if (discussionDlg.SelectedDiscussion != LoginDiscussionDlg.DummyDiscussion)
                res.discussion = discussionDlg.SelectedDiscussion;

            res.person = personDlg.SelectedPerson;

            return res;
        }

        private static void MsgParticipantsShouldSelectDiscussion()
        {
            MessageDlg.Show("Participants should choose a discussion they are invited to");
        }

        private static LoginResult LoginFlowEventGen()
        {
            SelectSession:
            //session
            var sessionDlg = new LoginSessionDlg();
            sessionDlg.ShowDialog();
            if (sessionDlg.SelectedSession == null)
                return null;

            SelectDiscussion:
            //discussion
            var discussionDlg = new LoginDiscussionDlg((Person) null);
            discussionDlg.ShowDialog();

            if (discussionDlg.BackClicked)
                goto SelectSession;

            if (discussionDlg.SelectedDiscussion == null)
            {
                MsgParticipantsShouldSelectDiscussion();
                return null;
            }

            //device type
            var devTypeDlg = new LoginDevTypeDlg(null);
            devTypeDlg.ShowDialog();

            if (devTypeDlg.BackClicked)
                goto SelectDiscussion;

            var res = new LoginResult();

            //build result           
            if (discussionDlg.SelectedDiscussion != LoginDiscussionDlg.DummyDiscussion)
                res.discussion = discussionDlg.SelectedDiscussion;

            res.session = sessionDlg.SelectedSession;
            res.devType = devTypeDlg.SelectedDeviceType;

            return res;
        }

        private static LoginResult LoginFlowExperiment()
        {
            SelectSession:
            //session
            var sessionDlg = new LoginSessionDlg();
            sessionDlg.ShowDialog();
            if (sessionDlg.SelectedSession == null)
                return null;

            SelectSeat:
            //seat 
            var seatDlg = new LoginSeatSelectorDlg();
            seatDlg.ShowDialog();

            if (seatDlg.BackClicked)
                goto SelectSession;

            if (seatDlg.SelectedSeat == null)
                return null;

            if (placeAlreadyBusy(sessionDlg.SelectedSession, seatDlg.SelectedSeat))
            {
                MsgPlaceReserved();
                goto SelectSession;
            }

            //EnterName:
            //name
            var nameDlg = new LoginName(seatDlg.SelectedSeat);
            nameDlg.ShowDialog();
            if (nameDlg.BackClicked)
                goto SelectSeat;

            if (nameDlg.EnteredName == null)
                return null;

            //if (!NameUnique(nameDlg.EnteredName, sessionDlg.SelectedSession))
            //{
            //    MsgNameRegistered();
            //    goto EnterName;
            //}

            //discussion selector
            var discussionDlg = new LoginDiscussionDlg(nameDlg.EnteredName);
            discussionDlg.ShowDialog();

            if (discussionDlg.BackClicked)
                goto SelectSeat;

            if (discussionDlg.SelectedDiscussion == null)
            {
                MsgParticipantsShouldSelectDiscussion();
                return null;
            }

            //final checks and build login result
            if (placeAlreadyBusy(sessionDlg.SelectedSession, seatDlg.SelectedSeat))
            {
                MsgPlaceReserved();
                goto SelectSession;
            }
            //if (!NameUnique(nameDlg.EnteredName, sessionDlg.SelectedSession))
            //{
            //    MsgNameRegistered();
            //    goto EnterName;
            //}

            //register user and build result
            var res = new LoginResult();
            if (discussionDlg.SelectedDiscussion != LoginDiscussionDlg.DummyDiscussion)
                res.discussion = discussionDlg.SelectedDiscussion;

            res.session = sessionDlg.SelectedSession;

            res.person = RegisterOrLogin(nameDlg.EnteredName,
                                         discussionDlg.SelectedDiscussion,
                                         sessionDlg.SelectedSession,
                                         seatDlg.SelectedSeat);

            return res;
        }

        //if given seat was not used in current session, and user takes the seat, new user is created in DB.
        //if user takes seat that was used in this session, then no new user is created. instead, the user 
        //is recognized as the same user who took the seat in current session, though during second login user 
        //enters name again (effectively changing name)
        private static Person RegisterOrLogin(string name, Discussion discussion, Session session, Seat seat)
        {
            //was the seat taken by some user? 
            var sessionId = session.Id;
            var seatId = seat.Id;
            DbCtx.DropContext();
            var outrunnerPerson =
                DbCtx.Get().Person.FirstOrDefault(p0 => p0.Session != null && p0.Session.Id == sessionId &&
                                                        p0.Seat != null && p0.Seat.Id == seatId);

            //the user already took the place, just change name
            if (outrunnerPerson != null)
            {
                outrunnerPerson.Name = name;

                //do we need general side ? 
                var ctx = DbCtx.Get();
                var previousGenSide = ctx.GeneralSide.FirstOrDefault(gs0 => gs0.Discussion.Id == discussion.Id &&
                                                                            gs0.Person.Id == outrunnerPerson.Id);
                if (previousGenSide == null)
                {
                    //the person takes part in this discussion first time, create general 
                    //side of the person in this discussion
                    var disc = ctx.Discussion.FirstOrDefault(d0 => d0.Id == discussion.Id);
                    outrunnerPerson.GeneralSide.Add(
                        CreateGeneralSide(
                            outrunnerPerson,
                            disc,
                            (int) SideCode.Neutral
                            )
                        );

                    //assign person to all topics of selected discussion
                    foreach (var topic in disc.Topic)
                        outrunnerPerson.Topic.Add(topic);
                }

                DbCtx.Get().SaveChanges();

                return outrunnerPerson;
            }
            else
            {
                //seat was not used in this session, create new user
                var ctx = DbCtx.Get();
                var p = new Person();
                p.Name = name;
                p.Session = ctx.Session.FirstOrDefault(s0 => s0.Id == session.Id);
                p.Seat = ctx.Seat.FirstOrDefault(s0 => s0.Id == seat.Id);

                var disc = ctx.Discussion.FirstOrDefault(d0 => d0.Id == discussion.Id);
                p.GeneralSide.Add(CreateGeneralSide(p, disc, (int) SideCode.Neutral));

                //person inherits color of seat
                p.Color = p.Seat.Color;

                p.Email = "no-email";

                //assign person to all topics of selected discussion
                foreach (var topic in disc.Topic)
                    p.Topic.Add(topic);

                ctx.AddToPerson(p);
                DbCtx.Get().SaveChanges();
                return p;
            }
        }

        private static GeneralSide CreateGeneralSide(Person p, Discussion d, int side)
        {
            var genSide = new GeneralSide();
            genSide.Side = (int) SideCode.Neutral;
            genSide.Discussion = d;
            genSide.Person = p;
            return genSide;
        }

        //private static void MsgNameRegistered()
        //{
        //    MessageBox.Show("This name is already registered in this session", "Error", 
        //        MessageBoxButton.OK, MessageBoxImage.Error);
        //}

        private static void MsgPlaceReserved()
        {
            MessageDlg.Show("Some online user has taken selected seat in current session",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        //static bool NameUnique(string Name, Session session)
        //{
        //    var sessionId = session.Id;
        //    DbCtx.DropContext();
        //    var outrunnerPerson = DbCtx.Get().Person.FirstOrDefault(p0=>p0.Name==Name && p0.Session != null && p0.Session.Id == sessionId);
        //    return outrunnerPerson == null;
        //}

        private static bool placeAlreadyBusy(Session session, Seat seat)
        {
            var sessionId = session.Id;
            var seatId = seat.Id;
            DbCtx.DropContext();
            var outrunnerPerson = DbCtx.Get().Person.FirstOrDefault(p0 => p0.Online &&
                                                                          p0.Session != null &&
                                                                          p0.Session.Id == sessionId &&
                                                                          p0.Seat != null && p0.Seat.Id == seatId);

            //only if outrunner person exists and online, the place is busy
            return (outrunnerPerson != null);
        }
    }
}
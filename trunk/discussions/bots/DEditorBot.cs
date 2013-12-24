using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DistributedEditor;

namespace Discussions.bots
{
    public class DEditorBot : IDisposable
    {
        private readonly SceneManager _sceneManager;
        private readonly Random _rnd;
        private bool _active = true;

        public DEditorBot(SceneManager sceneManager)
        {
            _sceneManager = sceneManager;

            _rnd = new Random();

            Launch();
        }

        async void Launch()
        {
            while (_active)
            {
                await WorkAsync();
                await Utils.DelayAsync(30);
            }
        }

        async Task WorkAsync()
        {
            var r = _rnd.Next(3);
            if (r == 0)
            {
                //delete

                try
                {
                    var textShapes =
                        _sceneManager.Doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.Text).ToArray();
                    if (textShapes.Length > 0)
                    {
                        int toDelete = _rnd.Next(textShapes.Length);
                        var txt = (VdText)textShapes[toDelete];

                        _sceneManager.InpDeviceUp(new Point());

                        await Utils.DelayAsync(100);

                        _sceneManager.LockIfPossible(txt, null, new Point(), null);

                        await Utils.DelayAsync(300);

                        _sceneManager.RemoveShape(SessionInfo.Get().person.Id, supressErrMessages: true);
                    }
                }
                catch
                {
                    int i = 0;
                }
            }
            else if (r == 1)
            {
                try
                {

                    //edit
                    var textShapes = _sceneManager.Doc.GetShapes().Where(sh => sh.ShapeCode() == VdShapeType.Text).ToArray();
                    if (textShapes.Length > 0)
                    {
                        int toEdit = _rnd.Next(textShapes.Length);
                        var txt = (VdText)textShapes[toEdit];

                        _sceneManager.InpDeviceUp(new Point());

                        await Utils.DelayAsync(100);

                        _sceneManager.LockIfPossible(txt, null, new Point(), null);

                        await Utils.DelayAsync(300);

                        for (int i = 0; i < 6; i++)
                        {
                            txt.Text = txt.Text + _rnd.Next(9);
                            txt.BotEnableTextSerialization();
                            _sceneManager.SendSyncState(txt);
                            await Utils.DelayAsync(50);
                        }
                        txt.Text = "bot" + _rnd.Next();
                        txt.BotEnableTextSerialization();
                        _sceneManager.SendSyncState(txt);
                        await Utils.DelayAsync(50);

                        txt.Text = "bot" + _rnd.Next();
                        txt.BotEnableTextSerialization();
                        _sceneManager.SendSyncState(txt);
                        await Utils.DelayAsync(50);
                    }
                }
                catch
                {
                    int i = 0;
                }
            }
            else
            {
                try
                {
                    //create
                    _sceneManager.Palette.SelectText();

                    _sceneManager.InpDeviceDown(new Point(_rnd.Next(1300), _rnd.Next(1000)), null);

                    await Utils.DelayAsync(300);

                    _sceneManager.InpDeviceUp(new Point());
                }
                catch
                {
                    int i = 0;
                }
            }
        }


        public void Dispose()
        {
            _active = false;
        }
    }
}
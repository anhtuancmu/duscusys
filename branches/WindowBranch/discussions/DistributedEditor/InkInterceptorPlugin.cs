using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input.StylusPlugIns;

namespace DistributedEditor
{
    class InkInterceptorPlugin : System.Windows.Input.StylusPlugIns.StylusPlugIn
    {
        protected override void OnStylusDown(RawStylusInput rawStylusInput)
        {
            base.OnStylusDown(rawStylusInput);
            HandlePoint(rawStylusInput);
        }

        public void PlayStylusDown(RawStylusInput rawStylusInput)
        {
            base.OnStylusDown(rawStylusInput);
        }

        protected override void OnStylusMove(RawStylusInput rawStylusInput)
        {
            base.OnStylusMove(rawStylusInput); 
            HandlePoint(rawStylusInput);
        }

        public void PlayStylusMove(RawStylusInput rawStylusInput)
        {
            base.OnStylusMove(rawStylusInput);
        }

        protected override void OnStylusUp(RawStylusInput rawStylusInput)
        {
            base.OnStylusUp(rawStylusInput);    
            HandlePoint(rawStylusInput);
        }

        public void PlayStylusUp(RawStylusInput rawStylusInput)
        {
            base.OnStylusUp(rawStylusInput);
        }

        void HandlePoint(RawStylusInput rawStylusInput)
        {            
            //Console.WriteLine("num points {0}", rawStylusInput.GetStylusPoints().Count());
            //foreach (var p in rawStylusInput.GetStylusPoints())          
            //    Console.WriteLine("{0}-{1}", p.X, p.Y);            
        }
    }
}

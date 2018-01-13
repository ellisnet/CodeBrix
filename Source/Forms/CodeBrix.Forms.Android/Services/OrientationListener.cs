/* Copyright 2018 Ellisnet - Jeremy Ellis (jeremy@ellisnet.com)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Views;
using CodeBrix.Messages;
using Xamarin.Forms;

namespace CodeBrix.Forms.Services
{
    public class OrientationListener : OrientationEventListener
    {
        private int _notificationOrientation;
        private DateTime _notificationTimer;
        private DateTime _lastSetOrientation;

        private int _orientation;
        public int Orientation => _orientation;

        private bool _firstOrientationSet;
        public bool FirstOrientationSet => _firstOrientationSet;

        private bool _enabled;
        public bool Enabled => _enabled;

        public OrientationListener(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        { }

        public OrientationListener(Context context) : base(context)
        { }

        public OrientationListener(Context context, SensorDelay rate) : base(context, rate)
        { }

        public override void OnOrientationChanged(int orientation)
        {
            int immediateOrientation = (int)(90 * (Math.Round((double)orientation / 90)));
            immediateOrientation = (immediateOrientation == 0 || immediateOrientation == 360) ? 0 : immediateOrientation;

            if (immediateOrientation != _orientation &&
                DateTime.Now.Subtract(_lastSetOrientation).TotalMilliseconds > 100)
            {
                _orientation = immediateOrientation;
                _lastSetOrientation = DateTime.Now;
            }

            if (_orientation != _notificationOrientation
                && DateTime.Now.Subtract(_notificationTimer).TotalMilliseconds > 1000)
            {
                _notificationOrientation = _orientation;
                _notificationTimer = DateTime.Now;
                MessagingCenter.Instance.Send(new ScreenSizeMessage(_orientation == 0 || _orientation == 180), ScreenSizeMessage.OnSizeAllocated);
            }
            else if (_orientation == _notificationOrientation)
            {
                _notificationTimer = DateTime.Now;
            }

            _firstOrientationSet = true;
        }

        public bool TryEnabling()
        {
            if (_enabled) { return true; }
            if (CanDetectOrientation())
            {
                Enable();
                _enabled = true;
            }
            return _enabled;
        }
    }
}

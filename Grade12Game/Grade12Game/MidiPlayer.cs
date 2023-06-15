using System;
using System.Threading;
using System.Runtime.InteropServices;


namespace Grade12Game
{
    // Native API
    internal static class NativeMethods
    {
        internal delegate void MidiCallback(IntPtr handle, int msg, int instance, int param1, int param2);
        [DllImport("winmm.dll")]
        internal static extern int midiOutOpen(
          ref IntPtr handle,
          int deviceID,
          MidiCallback proc,
          int instance,
          int flags
        );
        [DllImport("winmm.dll")]
        internal static extern int midiOutShortMsg(IntPtr handle, uint message);
    }

    // Player
    class MidiPlayer : IDisposable
    {
        // Internal Bridge
        private bool isDisposed;
        private IntPtr handle;
        // Constructor
        public MidiPlayer()
        {
            // Create Thread
            int result = NativeMethods.midiOutOpen(ref handle, 0, null, 0, 0);
        }
        private void ShootNoise(Object stateInfo)
        {
            // Play shoot on channel 16 at max volume
            this.programChange(30, 127);
            this.noteOn(15, 50, 100);
        }
        // FrameWork
        public void changeController(int channel, byte controllerType, byte value)
        {
            // Send Properperty Change, need to make a binary package of command, channel, instrument
            NativeMethods.midiOutShortMsg(handle, (uint)((0xb0 | channel) | (controllerType << 8) | (value << 16)));
        }
        public void programChange(int channel, byte sound)
        {
            // Send Instrument Change, need to make a binary package of command, channel, instrument
            NativeMethods.midiOutShortMsg(handle, (uint)((0xc0 | channel) | (sound << 8)));
        }
        public void pitchBend(int channel, int value)
        {
            // Send Pitch Bend, need to make a binary package of command, channel, instrument, low value bits, high value bits
            NativeMethods.midiOutShortMsg(handle, (uint)((0xe0 | channel) | ((value >> 16) << 8) | ((value >> 8) << 16)));
        }
        public void noteOn(int channel, byte noteNumber, byte velocity)
        {
            // Send Note On, need to make a binary package of command, channel, note, velocity
            NativeMethods.midiOutShortMsg(handle, (uint)((0x90 | channel) | (noteNumber << 8) | (velocity << 16)));
        }
        public void noteOff(int channel, byte noteNumber)
        {
            // Send Note On, need to make a binary package of command, channel, note, velocity
            NativeMethods.midiOutShortMsg(handle, (uint)((0x80 | channel) | (noteNumber << 8)));
        }
        // Songs
        //Internal Music Manager
        private void handleMusic(Object stateInfo)
        {
            // TODO: Maybe Different Songs
            Songs.Doom(this);
        }
        // External API
        public void ShootNoise()
        {
            // Try Playing On A Seperate Thread
            ThreadPool.QueueUserWorkItem(ShootNoise, "ThreadPool");
        }
        // Music Manager
        public void ManageMusic()
        {
            ThreadPool.QueueUserWorkItem(handleMusic, "ThreadPool");
        }
        // Dispoable
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            // free native resources if there are any.
            if (handle != IntPtr.Zero)
            {
                //Marshal.FreeHGlobal(handle);
                handle = IntPtr.Zero;
            }
            isDisposed = true;
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't
        // own unmanaged resources, but leave the other methods
        // exactly as they are.
        ~MidiPlayer()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
    }
}

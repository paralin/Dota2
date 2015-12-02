/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/net/Subchannel.cs
*/

namespace Dota2.GameClient.Engine.Session.Networking
{
    /// <summary>
    ///     A DOTA 2 DotaGameConnection subchannel.
    /// </summary>
    internal class Subchannel
    {
        public enum SubchannelState
        {
            Empty,
            Queued,
            Blocked
        }

        private int count;
        private byte[] data;
        private int offset;

        private Subchannel(uint index)
        {
            State = SubchannelState.Empty;
            Index = index;
        }

        public SubchannelState State { get; private set; }

        public bool Empty
        {
            get { return State == SubchannelState.Empty; }
        }

        public bool Queued
        {
            get { return State == SubchannelState.Queued; }
        }

        public bool Blocked
        {
            get { return State == SubchannelState.Blocked; }
        }

        public uint Index { get; private set; }
        public uint SentIn { get; private set; }

        public static Subchannel Create(uint index)
        {
            return new Subchannel(index);
        }

        public void Clear()
        {
            State = SubchannelState.Empty;
        }

        public void Queue(byte[] data, int offset, int count)
        {
            this.data = data;
            this.offset = offset;
            this.count = count;

            State = SubchannelState.Queued;
        }

        public void Requeue()
        {
            State = SubchannelState.Queued;
        }

        public void Write(DotaGameConnection.Packet packet)
        {
            if (State == SubchannelState.Queued)
            {
                packet.Stream.WriteBool(true);
                WriteQueued(packet);
            }
            else
            {
                packet.Stream.WriteBool(false);
            }
        }

        private void WriteQueued(DotaGameConnection.Packet packet)
        {
            if (offset != 0 || count != data.Length)
            {
                packet.Stream.WriteBool(true);
                WriteQueuedChunk(packet);
            }
            else
            {
                packet.Stream.WriteBool(false);
                WriteQueuedSingle(packet);
            }

            SentIn = packet.Seq;
            State = SubchannelState.Blocked;
        }

        private void WriteQueuedChunk(DotaGameConnection.Packet packet)
        {
            var chunkOffset = (offset + DotaGameConnection.BYTES_PER_CHUNK - 1)/DotaGameConnection.BYTES_PER_CHUNK;
            var chunkCount = (count + DotaGameConnection.BYTES_PER_CHUNK - 1)/DotaGameConnection.BYTES_PER_CHUNK;

            packet.Stream.WriteBits((uint) chunkOffset, 18);
            packet.Stream.WriteBits((uint) chunkCount, 3);

            if (offset == 0)
            {
                packet.Stream.WriteBool(false);
                packet.Stream.WriteBool(false);

                packet.Stream.WriteBits((uint) data.Length, 26);
            }

            packet.Stream.Write(data, offset, count);
        }

        private void WriteQueuedSingle(DotaGameConnection.Packet packet)
        {
            packet.Stream.WriteBool(false);

            packet.Stream.WriteBits((uint) count, 18);
            packet.Stream.Write(data, offset, count);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheSTAR.Utility
{
    [Serializable]
    public struct GameTimeSpan
    {
        #region Fields

        private ushort hours;
        private byte minutes;
        private byte seconds;

        public ushort Hours => hours;
        public byte Minutes => minutes;
        public byte Seconds => seconds;

        private const byte SIXTY = 60;

        #endregion Fields

        #region Constructors

        public GameTimeSpan(TimeSpan timeSpan)
        {
            hours = (ushort)timeSpan.Hours;
            minutes = (byte)timeSpan.Minutes;
            seconds = (byte)timeSpan.Seconds;
        }

        public GameTimeSpan(ushort h, byte m, byte s)
        {
            hours = h;
            minutes = m;
            seconds = s;

            FormatValuesCounts();
        }

        #endregion Constructors

        #region Operators

        public static GameTimeSpan operator + (GameTimeSpan a, GameTimeSpan b)
        {
            GameTimeSpan value = new(
                (ushort)(a.hours + b.hours), 
                (byte)(a.minutes + b.minutes), 
                (byte)(a.seconds + b.seconds));
            
            value.FormatValuesCounts();

            return value;
        }
        
        public static implicit operator GameTimeSpan(TimeSpan timeSpan)
        {
            return new GameTimeSpan(timeSpan);
        }

        #endregion Operators

        #region Overrides

        public override string ToString()
        {
            return $"{hours}:{minutes}:{seconds}";
        }

        #endregion Overrides

        #region Format

        private void FormatValuesCounts()
        {
            while (seconds >= SIXTY)
            {
                seconds -= SIXTY;
                minutes++;
            }

            while (minutes >= SIXTY)
            {
                minutes -= SIXTY;
                hours++;
            }
        }

        public string FormatForText()
        {
            if (hours > 0) return $"{hours}h";
            return $"{minutes}m";
        }
    
        #endregion Format
    }
}
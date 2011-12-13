using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;

namespace AutoBot.Host
{

    internal class AutoBotRawUserInterface : PSHostRawUserInterface
    {

        #region Fields

        private Size m_BufferSize = new Size(80, 25);
        private ConsoleColor m_BackgroundColor = ConsoleColor.Black;
        private ConsoleColor m_ForegroundColor = ConsoleColor.White;
        private Coordinates m_CursorPosition = new Coordinates(0, 0);
        private int m_CursorSize = 1;

        #endregion

        #region PSHostRawUserInterface Members

        public override ConsoleColor BackgroundColor
        {
            get
            {
                return m_BackgroundColor;
            }
            set
            {
                m_BackgroundColor = value;
            }
        }

        public override Size BufferSize
        {
            get
            {
                return m_BufferSize;
            }
            set
            {
                m_BufferSize = value;
            }
        }

        public override Coordinates CursorPosition
        {
            get
            {
                return m_CursorPosition;
            }
            set
            {
                m_CursorPosition = value;
            }
        }

        public override int CursorSize
        {
            get
            {
                return m_CursorSize;
            }
            set
            {
                m_CursorSize = value;
            }
        }

        public override void FlushInputBuffer()
        {
            throw new NotImplementedException();
        }

        public override ConsoleColor ForegroundColor
        {
            get
            {
                return m_ForegroundColor;
            }
            set
            {
                m_ForegroundColor = value;
            }
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException();
        }

        public override bool KeyAvailable
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException();
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
            throw new NotImplementedException();
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
            throw new NotImplementedException();
        }

        public override Coordinates WindowPosition
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Size WindowSize
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string WindowTitle
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

    }

}

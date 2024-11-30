using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XRat
{
    public static class Utils
    {
        [DllImport("user32.dll")]
        private static extern int BlockInput(int fBlockIt);

        public static void BlockKeyboard()
        {
            // Блокируем клавиатуру
            BlockInput(1);
        }

        public static void UnblockKeyboard()
        {
            // Разблокируем клавиатуру
            BlockInput(0);
        }
    }
}

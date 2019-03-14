using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace JeopardyCore
{
    class JeoController
    {
        Random rando = new Random();
        private JeoModel game = new JeoModel();

        public JeoController()
        {
            string check = game.CatByName("INTERNATdfIONAL SPORTS").CatName;
            Console.WriteLine(check);
            Console.Read();
        }
    }
}

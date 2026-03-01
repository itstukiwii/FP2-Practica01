// Marta Reyes Funk
// Diego González Martin 
// (Hermes)
// Practica 1: Unblock Me

namespace FP2_Practica01
{
    internal class Program
    {
        struct Coor
        { // representacion de posiciones y direcciones de desplazamiento
            public int x, y;
        }

        struct Estado
        { // estado del juego
            public char[,] mat; // # es muro, . es libre, a y b son bloques 
            public char obj;    // char del bloque objetivo
            public Coor act, sal; //posición del cursor y de la salida
            public bool sel;   // indica si hay bloque seleccionado para mover o no
        }

        static void Main()
        {
            Console.WriteLine("Introduce el numero de nivel que quieres jugar:");
            int n = int.Parse(Console.ReadLine());
            Estado e = LeeNivel("levels.txt", n); // se lee el nivel del archivo y se guarda en un estado
            Render(e); // se renderiza el estado inicial




        }
        static Estado LeeNivel(string file, int n)
        {
            bool enc = false; // indica si se ha encontrado el nivel con su final
            Estado e = new Estado();
            e.sel = false; // de primeras no hay bloque seleccionado
            e.act.x = 1;
            e.act.y = 1; // el cursor empieza en la esquina superior izquierda dentro del borde
            int l, c;    // valores auxiliares para saber de qué tamaño hacer mat
            MideTablero(out l, out c, file, n); // se mide el tablero para crear la matriz del tamaño adecuado
            e.mat = new char[l, c];             // se crea la matriz del tamaño correspondiente
            e.sal.x = 0;   // se usa otro metodo luego para encontrar la salida    
            e.sal.y = 0;   // por el momento se inicializa a 0,0 
            StreamReader sr = new StreamReader(file);
            string lvl = "level " + n.ToString();      // los niveles están como "level n" en el archivo, por lo que se busca esa línea
            while (!sr.EndOfStream && !enc)
            {
                string linea = sr.ReadLine();
                if (linea == lvl)
                {              // se ha encontrado el nivel
                    e.obj = sr.ReadLine()[0];   // el bloque objetivo es el primer caracter de la siguiente linea
                    for (int i = 0; i < e.mat.GetLength(0); i++)
                    {
                        e.mat[i, 0] = '#'; // se rellena el borde izquierdo de la matriz con # para los muros
                    }
                    for (int j = 1; j < e.mat.GetLength(1) - 1; j++)
                    {
                        string s = sr.ReadLine();
                        e.mat[0, j] = '#'; // se rellena el borde superior de la matriz con # para los muros
                        for (int i = 1; i < e.mat.GetLength(0) - 1; i++)
                        {
                            e.mat[i, j] = s[i - 1]; // se rellena el interior de la matriz con los caracteres del nivel
                        }
                        e.mat[e.mat.GetLength(0) - 1, j] = '#'; // se rellena el borde inferior de la matriz con # para los muros
                        if (s == "") enc = true; // el nivel termina con una linea en blanco, se sale del bucle
                    }
                    for (int i = 0; i < e.mat.GetLength(0); i++)
                    {
                        e.mat[i, e.mat.GetLength(1) - 1] = '#'; // se rellena el borde derecho de la matriz con # para los muros
                    }
                }
            }
            Marcasalida(ref e); // se marca la salida en la matriz y se guarda su posición en e.sal
            sr.Close();
            return e;
        }
        static void MideTablero(out int l, out int c, string file, int n)
        {
            StreamReader sr = new StreamReader(file);
            bool enc = false;
            l = 0; c = 0;
            while (!sr.EndOfStream && !enc)
            {
                string s = sr.ReadLine();
                if (s == "level " + n)     // se ha encontrado el nivel concreto
                {
                    sr.ReadLine();           // se lee la linea del bloque objetivo, que no es relevante para medir el tablero
                    while (!sr.EndOfStream && !enc)
                    {
                        string ss = sr.ReadLine();
                        if (ss == "") enc = true;    // el nivel termina con una linea en blanco, se sale del bucle
                        else
                        {
                            l++;           // se cuenta una fila más
                            c = ss.Length; // se actualiza el número de columnas, aunque siempre será el mximo
                        }

                    }
                }
            }
            l = l + 2; // se añaden las filas del borde
            c = c + 2; // se añaden las columnas del borde
        }
        static void Marcasalida(ref Estado est)
        {
            for (int j = 0; j < est.mat.GetLength(1); j++)
            {
                for (int i = 0; i < est.mat.GetLength(0); i++)
                {
                    if (est.mat[i, j] == est.obj) // se encuentra el bloque objetivo, por el orden del bucle, siempre será la primera instancia del bloque objetivo
                    {
                        if (est.mat[i, j + 1] == est.obj) // se comprueba si el bloque objetivo es vertical
                        {
                            est.mat[i, est.mat.GetLength(1) - 1] = '.'; // si el bloque objetivo es vertical, se marca la salida en la parte inferior de la columna
                            est.sal.x = i; // se guarda la posición de la salida
                            est.sal.y = est.mat.GetLength(1) - 1;
                        }
                        else // si el bloque objetivo no es vertical, debe ser horizontal
                        {
                            est.mat[est.mat.GetLength(0) - 1, j] = '.'; // si el bloque objetivo es horizontal, se marca la salida en la parte derecha de la fila
                            est.sal.x = est.mat.GetLength(0) - 1;
                            est.sal.y = j;
                        }
                    }
                }
            }
        }

        static void Render(Estado est)
        {
            Console.Clear(); 
            ConsoleColor[] colores = (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor)); // array con todos los colores de consola
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            for (int j = 0; j < est.mat.GetLength(1); j++)
            {
                if (j > 0) Console.WriteLine(""); 
                for (int i = 0; i < est.mat.GetLength(0); i++)
                {
                    if (est.mat[i, j] == '#') Console.ForegroundColor = colores[colores.Length - 1]; // el muro se pinta de blanco, último color del array
                    else if (est.mat[i, j] == '.') Console.ForegroundColor = colores[0]; // el espacio libre se pinta de negro, primer color del array
                    else if (est.mat[i, j] >= 'a' || est.mat[i, j] <= 'z') Console.ForegroundColor = BloqueToInt(est.mat[i, j], colores);
                    // cada letra corresponde a bloques distintos y cada bloque tiene un color distinto, este se consigue con el método auxiliar
                    Console.Write("██");
                }
            }
            Console.SetCursorPosition(est.act.x * 2, est.act.y); // se daja espacio entre columnas para que se vea mejor visualmente
            Console.ForegroundColor = BloqueToInt(est.mat[est.act.x, est.act.y], colores);
            if (est.sel) Console.Write("**"); // se pinta el cursor de seleccionado
            else Console.Write("<>");         // o el cursor de no seleccionado
            Console.SetCursorPosition(2, est.mat.GetLength(1) + 1); // se pinta el objetivo, dejando espacio respecto al muro (por eso el +1)
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Objetivo: ");
            Console.ForegroundColor = BloqueToInt(est.obj, colores); // el bloque objetivo se pinta del color correspondiente
            Console.Write("██");
            Console.ResetColor();
        }
        static ConsoleColor BloqueToInt(char c, ConsoleColor[] color) // se accede directamente al enum, no solo al número
        { 
            return color[(int)c - (int)'a' + 1]; // a = 1, b = 2, etc, el +1 es para que el bloque a no se corresponda con el color negro que es el de los espacios
        }
        /*static int BloqueToInt(char c)
        {
            return ((int)c) - ((int)'a') + 1; // a = 1, b = 2, etc
        }*/

        static char LeeInput()
        {
            char d = ' ';
            while (d == ' ')
            {
                if (Console.KeyAvailable)
                {
                    string tecla = Console.ReadKey().Key.ToString();
                    switch (tecla)
                    {
                        case "LeftArrow": d = 'l'; break; // direcciones
                        case "UpArrow": d = 'u'; break;
                        case "RightArrow": d = 'r'; break;
                        case "DownArrow": d = 'd'; break;
                        case "Delete": d = 'z'; break; // deshacer jugada
                        case "Escape": d = 'q'; break; // salir del juego
                        case "Spacebar": d = 's'; break; // seleccion de bloque
                    }
                }
            }
            return d;
        }
    }
}

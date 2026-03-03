// Marta Reyes Funk
// Diego González Martin 
// (Hermes)
// Practica 1: Unblock Me

using System;

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
        struct Jugada
        {
            public Coor cursor;
            public Coor dir;
        }
        struct Memoria
        {
            public Jugada[] jugadas;
            public int numJugadas;
        }

        static void Main()
        {
            Console.WriteLine("Introduce el numero de nivel que quieres jugar:");
            int n = int.Parse(Console.ReadLine());
            Estado e = LeeNivel("levels.txt", n); // se lee el nivel del archivo y se guarda en un estado
            Memoria mem = CreaMemoria(); // se crea la memoria para guardar las jugadas, luego se implementa el módulo de deshacer jugada

            bool fin = false; // indica si se ha ganado o no
            bool win = false; // indica si se ha ganado o no, se muestra un mensaje al ganar
            char tecla;       // tecla pulsada por el jugador

            Render(e, mem); // se renderiza el estado inicial
            while (!fin)
            {
                tecla = LeeInput(); // se lee la tecla pulsada por el jugador
                if (tecla == 'q') fin = true; // si se pulsa q, se sale del juego
                else
                {
                    ProcesaInput(ref e, tecla, ref mem); // se procesa la tecla pulsada y se actualiza el estado
                    Render(e, mem);                  // se renderiza el nuevo estado
                    if (e.mat[e.sal.x, e.sal.y] == e.obj) // se comprueba si el bloque objetivo ha llegado a la salida
                    {
                        fin = true; // se ha ganado, se sale del bucle
                        win = true; // se ha ganado, se muestra el mensaje de victoria
                    }
                }
            }
            if (win)
            {
                Console.WriteLine(); // se deja hueco entre el tablero y el mensaje de victoria
                Console.WriteLine("¡Has ganado! Yayy L(^w^)/"); // mensaje de victoria :3
            }



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


        static void Render(Estado est, Memoria mem)
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
            Console.BackgroundColor = BloqueToInt(est.mat[est.act.x, est.act.y], colores); // se deja el fondo del color que estaba para que mole más
            Console.ForegroundColor = ConsoleColor.White;
            if (est.sel) Console.Write("<>"); // se pinta el cursor de seleccionado
            else Console.Write("**");         // o el cursor de no seleccionado
            Console.BackgroundColor = ConsoleColor.Black; // lo reseteo

            Console.SetCursorPosition(1, est.mat.GetLength(1) + 1); // se pinta el objetivo, dejando espacio respecto al muro (por eso el +1)
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Objetivo: ");
            Console.ForegroundColor = BloqueToInt(est.obj, colores); // el bloque objetivo se pinta del color correspondiente
            Console.WriteLine("██");
            Console.ResetColor();
            Console.WriteLine(" Movimientos: " + mem.numJugadas); // se deja que el jugador pueda ver cuántas jugadas a realizado
        }
        static ConsoleColor BloqueToInt(char c, ConsoleColor[] color) // se accede directamente al enum, no solo al número
        {
            if (c == '.') return color[0]; // el espacio libre se pinta, para evitar excepciones al mover el cursor por un espacio libre
            return color[(int)c - (int)'a' + 1]; // a = 1, b = 2, etc, el +1 es para que el bloque a no se corresponda con el color negro que es el de los espacios
        }
        /*static int BloqueToInt(char c)
        {
            return ((int)c) - ((int)'a') + 1; // a = 1, b = 2, etc
        }*/


        static void MueveCursor(ref Estado est, Coor dir)
        {
            if (est.act.x - 1 == 0 && dir.x < 0) dir.x = 0;                        // evita salirse del tablero por la izquierda
            if (est.act.x + 1 == est.mat.GetLength(0) - 1 && dir.x > 0) dir.x = 0; // evita salirse del tablero por la derecha
            if (est.act.y + 1 == est.mat.GetLength(1) - 1 && dir.y > 0) dir.y = 0; // evita salirse del tablero por abajo
            if (est.act.y - 1 == 0 && dir.y < 0) dir.y = 0;                        // evita salirse del tablero por arriba
            est.act.x = est.act.x + dir.x; // se actualiza la posición del cursor
            est.act.y = est.act.y + dir.y; // lo mismo de arriba ||
        }
        static void MueveBloque(ref Estado est, Coor dir)
        {
            Coor[] blok;
            char c = est.mat[est.act.x, est.act.y]; // se guarda el char del bloque seleccionado
            blok = new Coor[MideBloque(est, c)];    // se crea un array de coordenadas del tamaño del bloque seleccionado
            EncuentraPorfa(est, blok, c);           // se rellenan las coordenadas de cada parte del bloque seleccionado en el array

            // comprobaciones para que el bloque no chocque con borde u otro bloque
            if (est.mat[blok[0].x - 1, est.act.y] != '.' && dir.x < 0) dir.x = 0;
            if (est.mat[blok[blok.Length - 1].x + 1, est.act.y] != '.' && dir.x > 0) dir.x = 0;
            if (est.mat[est.act.x, blok[0].y - 1] != '.' && dir.y < 0) dir.y = 0;
            if (est.mat[est.act.x, blok[blok.Length - 1].y + 1] != '.' && dir.y > 0) dir.y = 0;

            // al mover, se actualizan las posiciones 
            // se pinta bloque en la dirección deseada y se borra la parte de atrás

            // si el bloque es vertical
            if (est.mat[est.act.x + 1, est.act.y] == c || est.mat[est.act.x - 1, est.act.y] == c)
            {
                // si el bloque se mueve a la izquierda
                if (dir.x < 0 && dir.y == 0)
                {
                    est.mat[blok[0].x + dir.x, est.act.y] = c;
                    est.mat[blok[blok.Length - 1].x, est.act.y] = '.';
                    est.act.x = est.act.x + dir.x;
                }
                // si el bloque se mueve a la derecha
                else if (dir.x > 0 && dir.y == 0)
                {
                    est.mat[blok[blok.Length - 1].x + dir.x, est.act.y] = c;
                    est.mat[blok[0].x, est.act.y] = '.';
                    est.act.x = est.act.x + dir.x;
                }
            }
            else // si no es vertical, debe ser horizontal
            {
                // si el bloque se mueve hacia arriba
                if (dir.y < 0 && dir.x == 0)
                {
                    est.mat[est.act.x, blok[0].y + dir.y] = c;
                    est.mat[est.act.x, blok[blok.Length - 1].y] = '.';
                    est.act.y = est.act.y + dir.y;
                }
                // si el bloque se mueve hacia abajo
                else if (dir.y > 0 && dir.x == 0)
                {
                    est.mat[est.act.x, blok[blok.Length - 1].y + dir.y] = c;
                    est.mat[est.act.x, blok[0].y] = '.';
                    est.act.y = est.act.y + dir.y;
                }
            }
        }
        static int MideBloque(Estado est, char c) //mide el bloque seleccionado
        {
            // se cambió de recorrido a búsqueda para que escale mejor jeje

            bool seguir = true;
            if (est.mat[est.act.x + 1, est.act.y] == c || est.mat[est.act.x - 1, est.act.y] == c) // si el bloque es vertical, se cuenta cuántas filas ocupa
            {
                int top = est.act.x;
                while (seguir && top >= 0) // se empieza a contar desde la posición del bloque seleccionado, hacia arriba
                {
                    if (est.mat[top, est.act.y] == c) top--;
                    else seguir = false; // si se encuentra un caracter distinto, se ha terminado de contar el bloque, los bloques no están partidos OwO
                }
                seguir = true;
                int bot = est.act.x;
                while (seguir && bot < est.mat.GetLength(0)) // se sigue contando hacia abajo por si el bloque seleccionado no es la parte superior del bloque
                {
                    if (est.mat[bot, est.act.y] == c) bot++;
                    else seguir = false; // si se encuentra un caracter distinto, se ha terminado de contar el bloque, los bloques no están partidos OwO
                }
                return bot - top - 1; // se devuelve el número de filas que ocupa el bloque, restando 1 porque se ha contado la posición inicial dos veces

                /*for (int i = 0; i < est.mat.GetLength(0); i++)
                {
                    if (est.mat[i, est.act.y] == c) aux++;
                }*/
            }
            else  // si no es vertical, debe ser horizontal, se cuenta cuántas columnas ocupa
            {
                int izq = est.act.y;
                while (seguir && izq >= 0) // se empieza a contar desde la posición del bloque seleccionado, hacia la izquierda
                {
                    if (est.mat[est.act.x, izq] == c) izq--;
                    else seguir = false; // si se encuentra un caracter distinto, se ha terminado de contar el bloque, los bloques no están partidos :P
                }
                seguir = true;
                int der = est.act.y;
                while (seguir && der < est.mat.GetLength(1)) // se sigue contando hacia la derecha por si el bloque seleccionado no es la parte más a la izquierda del bloque
                {
                    if (est.mat[est.act.x, der] == c) der++;
                    else seguir = false; // si se encuentra un caracter distinto, se ha terminado de contar el bloque, los bloques no están partidos ;3
                }
                return der - izq - 1; // se devuelve el número de columnas que ocupa el bloque, restando 1 porque se ha contado la posición inicial dos veces

                /*for (int i = 0; i < est.mat.GetLength(1); i++)
                {
                    if (est.mat[est.act.x, i] == c) aux++;
                }*/
            }
        }
        static void EncuentraPorfa(Estado est, Coor[] n, char c) //este si funciona lo juro coño (fuerzas Hermes)
        {
            // se guardan las coordenadas de cada parte del bloque en un array de coordenadas
            // se cambió de recorrido a búsqueda para que escale mejor yippie ;D

            bool seguir = true;
            int aux = 0;
            // si el bloque es vertical
            if (est.mat[est.act.x + 1, est.act.y] == c || est.mat[est.act.x - 1, est.act.y] == c)
            {
                int start = est.act.x;
                // start va al principio del bloque para ser desde donde se empieza a guardar en el array de coordenadas
                while (seguir)
                {
                    start--;
                    if (est.mat[start, est.act.y] != c) seguir = false;
                }
                start++; // se vuelve a la posición del bloque, que es la última que se ha contado, para empezar a guardar las coordenadas
                seguir = true;
                while (seguir)
                {
                    n[aux].x = start;
                    n[aux].y = est.act.y;
                    aux++;
                    start++;
                    if (est.mat[start, est.act.y] != c) seguir = false;
                }

                /*for (int i = 0; i < est.mat.GetLength(0); i++)
                {
                    if (est.mat[i, est.act.y] == c) 
                    { 
                        n[aux].x = i; 
                        n[aux].y = est.act.y; 
                        aux++; 
                    }
                }*/
            }
            else // si no es vertical, debe ser horizontal
            {
                int start = est.act.y;
                while (seguir)
                {
                    start--;
                    if (est.mat[est.act.x, start] != c) seguir = false;
                }
                start++;
                seguir = true;
                while (seguir)
                {
                    n[aux].x = est.act.x;
                    n[aux].y = start;
                    aux++;
                    start++;
                    if (est.mat[est.act.x, start] != c) seguir = false;
                }

                /*for (int i = 0; i < est.mat.GetLength(1); i++)
                {
                    if (est.mat[est.act.x, i] == c) 
                    { 
                        n[aux].x = est.act.x; 
                        n[aux].y = i; 
                        aux++; 
                    }
                }*/
            }
        }


        static Memoria CreaMemoria()
        {
            Memoria mem;
            mem.jugadas = new Jugada[20]; // se crea un array con 20 jugada, luego se cambia con el módulo
            mem.numJugadas = 0;           // de primeras no hay jugadas en la memoria
            return mem;
        }
        static void GuardaJugada(ref Memoria mem, Coor cursor, Coor dir)
        {
            mem.jugadas[mem.numJugadas%mem.jugadas.Length].cursor = cursor; // se guarda la posición del cursor al mover el bloque
            mem.jugadas[mem.numJugadas%mem.jugadas.Length].dir = dir;       // se guarda la dirección en la que se ha movido el bloque
            mem.numJugadas++;                                               // se incrementa el número de jugadas en la memoria
        }
        static void InvierteDir(ref Coor dir) // método auxiliar para invertir la dirección al deshacer una jugada
        {
            dir.x = -dir.x;
            dir.y = -dir.y;
        }
        static void DeshaceJugada(ref Estado est, ref Memoria mem)
        {
            if (mem.numJugadas > 0) // solo se puede deshacer si hay jugadas en la memoria
            {
                Jugada last = mem.jugadas[(mem.numJugadas-1) % mem.jugadas.Length]; // se obtiene la última jugada
                est.act = last.cursor; // se vuelve a colocar el cursor en la posición que tenía al mover el bloque
                InvierteDir(ref last.dir); // se invierte la dirección para mover el bloque en la dirección contraria
                MueveBloque(ref est, last.dir); // se mueve el bloque en la dirección contraria para deshacer la jugada
                mem.numJugadas--; // se decrementa el número de jugadas en la memoria
            }
        }

        static void ProcesaInput(ref Estado est, char c, ref Memoria mem)
        {
            bool canMove = true; // avisa si no se puede mover el bloque, para no guardar la jugada en ese caso
            Coor dir;
            dir.x = 0; dir.y = 0; // se inicializa la dirección a 0,0 para luego actualizarla según la tecla pulsada

            if (c == 's')
            {
                if (est.mat[est.act.x, est.act.y] != '.') est.sel = !est.sel;
                // si se pulsa s, cambia el estado al contrario (solo si no está en un espacio libre)
            }
            switch (c)
            {
                // si está seleccionado, se mueve el bloque en la dirección indicada
                // si no está seleccionado, se mueve el cursor en la dirección indicada
                case 'u':
                    dir.y = -1; dir.x = 0;
                    if (est.sel)
                    {
                        Estado past = est; // se guarda para comparar
                        MueveBloque(ref est, dir);
                        if(past.act.x == est.act.x && past.act.y == est.act.y) canMove = false; // si el bloque no se ha movido
                        if(canMove) // solo se guarda la jugada si el bloque se ha movido
                        {
                            GuardaJugada(ref mem, est.act, dir); // se guarda la jugada en la memoria
                        }
                    }
                    else MueveCursor(ref est, dir);
                    break;
                case 'd':
                    dir.y = 1; dir.x = 0;
                    if (est.sel)
                    {
                        Estado past = est; // se guarda para comparar
                        MueveBloque(ref est, dir);
                        if (past.act.x == est.act.x && past.act.y == est.act.y) canMove = false; // si el bloque no se ha movido
                        if (canMove) // solo se guarda la jugada si el bloque se ha movido
                        {
                            GuardaJugada(ref mem, est.act, dir); // se guarda la jugada en la memoria
                        }
                    }
                    else MueveCursor(ref est, dir);
                    break;
                case 'r':
                    dir.y = 0; dir.x = 1;
                    if (est.sel)
                    {
                        Estado past = est; // se guarda para comparar
                        MueveBloque(ref est, dir);
                        if (past.act.x == est.act.x && past.act.y == est.act.y) canMove = false; // si el bloque no se ha movido
                        if (canMove) // solo se guarda la jugada si el bloque se ha movido
                        {
                            GuardaJugada(ref mem, est.act, dir); // se guarda la jugada en la memoria
                        }
                    }
                    else MueveCursor(ref est, dir);
                    break;
                case 'l':
                    dir.y = 0; dir.x = -1;
                    if (est.sel)
                    {
                        Estado past = est; // se guarda para comparar
                        MueveBloque(ref est, dir);
                        if (past.act.x == est.act.x && past.act.y == est.act.y) canMove = false; // si el bloque no se ha movido
                        if (canMove) // solo se guarda la jugada si el bloque se ha movido
                        {
                            GuardaJugada(ref mem, est.act, dir); // se guarda la jugada en la memoria
                        }
                    }
                    else MueveCursor(ref est, dir);
                    break;
                case 'z': // si se pulsa z, se deshace la última jugada
                    DeshaceJugada(ref est, ref mem);
                    break;
            }
        }
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

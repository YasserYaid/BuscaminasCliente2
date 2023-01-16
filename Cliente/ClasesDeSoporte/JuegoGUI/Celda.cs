namespace Cliente
{
    public class Celda
    {
        public TableroBuscaminas tableroJuego { get; set; }
        public int filaPosicion { get; set; }
        public int columnaPosicion { get; set; }
        public bool esMarcada { get; set; }
        public bool esMinada { get; set; }
        public bool esRevelada { get; set; }

        public Celda(TableroBuscaminas tableroJuego, int filaPosicion, int columnaPosicion)
        {
            this.tableroJuego = tableroJuego;
            this.filaPosicion = filaPosicion;
            this.columnaPosicion = columnaPosicion;
        }

        public int ContarMinasCeldasProximas()
        {
            int contadorBombas = 0;

            if (!esRevelada && !esMarcada)
            {
                esRevelada = true;

                for (int iterador = 0; iterador < 9; iterador++)
                {
                    if (iterador == 4) 
                    {
                        continue;
                    }
                    if (tableroJuego.VerificarSiCeldaEsBomba(filaPosicion + iterador / 3 - 1, columnaPosicion + iterador % 3 - 1)) 
                    {
                        contadorBombas++;
                    }
                }

                if (contadorBombas == 0)
                {
                    for (int iterador = 0; iterador < 9; iterador++)
                    {
                        if (iterador == 4) 
                        {
                            continue;
                        }
                        tableroJuego.DestaparCelda(filaPosicion + iterador / 3 - 1, columnaPosicion + iterador % 3 - 1);
                    }
                }
            }

            return contadorBombas;
        }
    }
}

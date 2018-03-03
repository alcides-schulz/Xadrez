using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enxadrista
{
    public class Historia
    {
        public Movimento Movimento;
        public bool RoqueE1G1;
        public bool RoqueE1C1;
        public bool RoqueE8G8;
        public bool RoqueE8C8;
        public int ContadorRegra50;
        public int IndiceEnPassant;
        public ulong Chave;

        public void Salva(Tabuleiro tabuleiro, Movimento movimento)
        {
            Movimento = movimento;
            RoqueE1G1 = tabuleiro.RoqueE1G1;
            RoqueE1C1 = tabuleiro.RoqueE1C1;
            RoqueE8G8 = tabuleiro.RoqueE8G8;
            RoqueE8C8 = tabuleiro.RoqueE8C8;
            ContadorRegra50 = tabuleiro.ContadorRegra50;
            IndiceEnPassant = tabuleiro.IndiceEnPassant;
            Chave = tabuleiro.Chave;
        }

        public void Restaura(Tabuleiro tabuleiro)
        {
            tabuleiro.RoqueE1G1 = RoqueE1G1;
            tabuleiro.RoqueE1C1 = RoqueE1C1;
            tabuleiro.RoqueE8G8 = RoqueE8G8;
            tabuleiro.RoqueE8C8 = RoqueE8C8;
            tabuleiro.ContadorRegra50 = ContadorRegra50;
            tabuleiro.IndiceEnPassant = IndiceEnPassant;
            tabuleiro.Chave = Chave;
        }
    }
}

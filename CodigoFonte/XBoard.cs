using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Enxadrista
{
    public class XBoard
    {
        public Motor Motor;

        public int ProfundidadeMaxima = Defs.PROFUNDIDADE_MAXIMA;
        public int MilisegundosPorMovimento = 10000; // 10 segundos
        
        private sbyte CorComputador = Defs.COR_NENHUMA;

        public XBoard(Motor motor)
        {
            Debug.Assert(motor != null);
            Motor = motor;
        }

        public void LoopPrincipal()
        {
            //Motor.Tabuleiro.NovaPartida("8/k/3p4/p2P1p2/P2P1P2/8/8/K7 w - - 0 1");
            //Motor.Tabuleiro.NovaPartida("2k5/8/1pP1K3/1P6/8/8/8/8 w - -");

            while (true) {
                if (Motor.Tabuleiro.CorJogar == CorComputador) { ProcuraMovimento(); }

                var linha = Console.ReadLine();

                if (linha == null) continue;
                if (linha == "quit") break;
                if (linha == "go") { CorComputador = Motor.Tabuleiro.CorJogar; continue; }
                if (linha == "new") { Comando_new(); continue; }
                if (linha == "force") { CorComputador = Defs.COR_NENHUMA; continue; }
                if (linha.StartsWith("st ")) { Comando_st(linha); continue; }
                if (linha.StartsWith("sd ")) { Comando_sd(linha); continue; }
                if (linha.StartsWith("time ")) { Comando_time(linha); continue; }
                if (linha == "undo") { Motor.Tabuleiro.DesfazMovimento(); continue; }
                if (linha == "post") { Motor.Pesquisa.ImprimeInformacao = true; continue; }
                if (linha == "nopost") { Motor.Pesquisa.ImprimeInformacao = false; continue; }

                if (linha == "i") { Console.WriteLine(Motor.Tabuleiro.ToString()); continue; }

                TentaExecutarMovimento(linha);
            }
        }

        private void ProcuraMovimento()
        {
            Motor.Pesquisa.LoopAprofundamentoInterativo(MilisegundosPorMovimento, ProfundidadeMaxima);
            if (Motor.Pesquisa.MelhorMovimento != null) {
                Motor.Tabuleiro.FazMovimento(Motor.Pesquisa.MelhorMovimento);
                Console.WriteLine("move " + Motor.Pesquisa.MelhorMovimento.Notacao());
                Console.Out.Flush();
            }
            else {
                CorComputador = Defs.COR_NENHUMA;
            }
        }

        private void TentaExecutarMovimento(string linha)
        {
            var lista = Motor.Tabuleiro.GeraMovimentos();
            foreach (var movimento in lista) {
                if (movimento.Notacao() == linha) {
                    Motor.Tabuleiro.FazMovimento(movimento);
                    if (!Motor.Tabuleiro.MovimentoFeitoLegal())
                        Motor.Tabuleiro.DesfazMovimento();
                }
            }
        }

        private void Comando_new()
        {
            Motor.Tabuleiro.NovaPartida(Defs.FEN_POSICAO_INICIAL);
            CorComputador = Defs.COR_NENHUMA;
        }

        private void Comando_st(string linha)
        {
            try {
                int tempo = int.Parse(linha.Substring("st ".Length));
                MilisegundosPorMovimento = tempo * 1000;
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }
        }

        private void Comando_sd(string linha)
        {
            try {
                int profundidade_maxima = int.Parse(linha.Substring("sd ".Length));
                ProfundidadeMaxima = profundidade_maxima;
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }
        }

        private void Comando_time(string linha)
        {
            try {
                int tempo_total = int.Parse(linha.Substring("time ".Length));
                MilisegundosPorMovimento = tempo_total * 10 / 30;
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}

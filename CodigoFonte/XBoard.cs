using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Enxadrista
{
    /// <summary>
    /// XBoard engine protocol.
    /// </summary>
    /// <remarks>
    /// Este módulo é responsável pela comunicação do motor de xadrez com o mundo
    /// exterior. O usuário pode enviar comandos para o motor e ler os resultados 
    /// de volta.
    /// Normalmente, você vai usar um programa que gerenciará a interface do 
    /// usuário, e este programa enviará comandos para o motor.
    /// 
    /// Protocolo do motor de xadrez
    /// ----------------------------
    /// Existem dois tipos de protocolos que você pode implementar no seu motor: Xboard ou UCI.
    /// Estamos usando o XBoard aqui porque este é o que eu sou mais familiar e também usei antes
    /// no meu outro motor de xadrez (Tucano).
    /// Muitos dos motores atuais está usando o UCI, mas o XBoard também é aceito.
    /// Dê uma olhada em ambos e veja qual deles você mais gosta, então faça sua decisão. Você
    /// pode olhar para os motores de xadrez existentes para a implementação real e também ler a
    /// documentação. Google "UCI chess engine protocol" ou "XBoard chess engine protocol".
    /// 
    /// Programa de Interface de xadrez
    /// -------------------------------
    /// Para o programa de interface do usuário também há várias opções, e você pode examiná-las
    /// e escolher a que mais gosta. Exemplos: Arena, Winboard, ChessGUI, Cutechess. Esses são 
    /// gratuitos, mas também existem os programas comerciais.
    /// Com estes programas, você pode jogar contra o motor, ou fazê-los jogar uns com os outros, 
    /// fazer torneios, etc. 
    /// Existem muitos torneios de motores de xadrez, onde você pode ter o seu motor. 
    /// Google "chess engine tournaments" ou "chess engines ratings".
    /// 
    /// Protocolo XBoard
    /// ----------------
    /// A implementação aqui é muito básica, e há muitos comandos que estão faltando. 
    /// Se você executar o Enxadrista, ele permitirá que você digite comandos seguinte o 
    /// formato Xboard. Quando começamos, vamos inicializar o tabuleiro de xadrez com um 
    /// novo jogo, e se você simplesmente digitar "go", o motor buscará o melhor movimento 
    /// para a posição. Ele imprimirá a linha principal de pesquisa e, no final, imprimirá 
    /// o melhor movimento encontrado.
    /// Aqui está um exemplo da posição inicial:
    /// 
    /// <code>
    /// Enxadrista 0.02 - Programa jogador de xadrez - Autor: Alcides Schulz
    /// go
    /// 1 8 0.0160 2 a2a3
    /// 1 16 0.0170 4 a2a4
    /// 1 18 0.0170 10 d2d3
    /// 1 36 0.0170 12 d2d4
    /// 2 0 0.0810 47 d2d4 d7d5
    /// 3 26 0.1440 169 d2d4 d7d5 c1f4
    /// 4 0 0.2090 1177 d2d4 d7d5 c1f4 c8f5
    /// 5 18 0.3420 6415 d2d4 d7d5 c1f4 c8f5 e2e3
    /// 5 26 0.3780 11499 e2e4 e7e5 d2d4 d7d5 f1b5
    /// 6 -10 0.5220 22983 a2a4 e7e5 b1c3 b8c6 e2e4 f8c5
    /// 6 0 0.5540 29929 d2d4 d7d5 c1f4 c8f5 b1c3 b8c6
    /// 6 8 0.6850 49446 e2e4 e7e5 g1f3 g8f6 f1c4 b8c6
    /// 7 8 1.0530 92157 a2a4 e7e5 e2e4 g8f6 g1f3 f8c5 b1c3
    /// 7 18 1.1680 119645 d2d4 d7d5 b1c3 c8f5 c1f4 b8c6 g1f3
    /// 8 8 2.5780 334213 d2d4 d7d5 c1f4 e7e6 e2e3 b8c6 f1b5 f8b4
    /// 9 8 5.3370 774563 a2a4 d7d5 d2d4 c8f5 b1c3 e7e6 e2e3 f8b4 f1b5
    /// 9 18 7.4010 1125629 d2d4 d7d5 e2e3 g8f6 b1c3 c8f5 g1f3 b8c6 f1b5
    /// move d2d4
    /// </code>
    /// 
    /// A primeira linha é a linha de identificação do motor.
    /// Eu digitei "go" e o motor imprimiu os resultados de pesquisa para cada profundidade.
    /// Observe que mudou de idéia durante a pesquisa, pois encontrou melhores movimentos.
    /// E a última linha foi o movimento encontrado na profundidade 9. 
    /// 
    /// Apenas para ter uma idéia, meu outro motor tucano produzirá os seguintes resultados:
    /// 
    /// <code>
    /// tucano chess engine by Alcides Schulz - 7.05 (type 'help' for information)
    ///     hash table: 64 MB, threads: 1
    ///     
    /// go
    /// Ply      Nodes  Score Time Principal Variation
    ///   1          2  -0.15  0.0 1. a2a3
    ///   1          4   0.00  0.0 1. b2b3
    ///   1          7   0.19  0.0 1. d2d3
    ///   1          9   0.28  0.0 1. e2e3
    ///   1         18   0.34  0.0 1. e2e4
    ///   1         23   0.43  0.0 1. g1f3
    ///   1         27   0.52  0.0 1. b1c3
    ///   2         54   0.10  0.0 1. b1c3 b8c6
    ///   3        188   0.46  0.0 1. b1c3 b8c6 e2e3
    ///   4        601   0.10  0.0 1. b1c3 b8c6 e2e3 e7e6
    ///   5       1763   0.28  0.0 1. b1c3 b8c6 e2e3 e7e6 g1f3
    ///   5       3341   0.30  0.0 1. d2d4 g8f6 b1c3 d7d5 g1f3
    ///   6       4532   0.10  0.0 1. d2d4 g8f6 b1c3 d7d5 g1f3 b8c6
    ///   6       6749   0.11  0.0 1. e2e4 b8c6 d2d4 d7d5 e4e5 e7e6
    ///   7      11660   0.21  0.0 1. e2e4 e7e5 g1f3 b8c6 d2d4 e5d4 f3d4
    ///   7      12949   0.25  0.0 1. b1c3 g8f6 g1f3 d7d5 d2d4 b8c6 c1f4
    ///   8      18340   0.10  0.0 1. b1c3 b8c6 d2d4 d7d5 g1f3 g8f6 c1f4 c8f5
    ///   8      23045   0.16  0.0 1. e2e4 e7e5 g1f3 b8c6 d2d4 e5d4 f3d4 g8f6
    ///   9      44751   0.00  0.0 1. e2e4 d7d5 e4d5 d8d5 b1c3 d5e6 g1e2 g8f6 d2d4 f6g4
    ///   9      49342   0.20  0.0 1. d2d4 g8f6 b1c3 d7d5 g1f3 b8c6 c1f4 c8f5 e2e3
    ///  10      70710   0.17  0.1 1. d2d4 g8f6 b1c3 d7d5 d1d3 b8c6 g1f3 e7e6 f3e5 f8d6
    ///  11     105405   0.22  0.1 1. d2d4 g8f6 b1c3 d7d5 g1f3 b8c6 e2e3 f6e4 f1d3 e4c3 b2c3
    ///  12     155177   0.21  0.1 1. d2d4 g8f6 b1c3 d7d5 g1f3 b8c6 e2e3 f6e4 f1d3 c8f5 e1g1 d8d6 g2g3
    ///  13     415003   0.25  0.4 1. d2d4 g8f6 g1f3 d7d5 e2e3 b8c6 f1d3 c8g4 b1c3 e7e5 d4e5 c6e5 e1g1 e5d3 c2d3
    ///  14     519157   0.13  0.5 1. d2d4 g8f6 g1f3 d7d5 e2e3 e7e6 f1d3 b8c6 e1g1 f8d6 b1c3 e8g8 f3g5 g7g6
    ///  14     698240   0.28  0.6 1. e2e4 e7e6 b1c3 d7d5 d2d4 b8c6 e4d5 e6d5 g1f3 g8f6 f1b5 f6e4 e1g1 f8d6 c3e4 d5e4
    ///  15    1064387   0.32  1.0 1. e2e4 e7e6 b1c3 d7d5 d2d4 b8c6 e4d5 e6d5 g1f3 g8f6 f1b5 f8d6 c1g5 e8g8 c3d5 d8e8 d5e3
    ///  16    1483641   0.28  1.3 1. e2e4 e7e6 b1c3 d7d5 d2d4 d5e4 c3e4 b8c6 g1f3 f8e7 c2c3 g8f6 e4f6 e7f6 f1d3 e6e5 d1e2
    ///  17    2603528   0.33  2.3 1. e2e4 b8c6 d2d4 d7d5 e4e5 g8h6 c1h6 g7h6 b1c3 c8f5 f1d3 c6d4 d3f5 d4f5 c3d5 c7c6 d5c3
    ///  18    4915810   0.22  4.4 1. e2e4 e7e5 g1f3 b8c6 f1b5 a7a6 b5c6 d7c6 d2d3 c8g4 e1g1 d8f6 b1c3 f8e7 c1e3 e8c8 a2a4 g4f3 d1f3 f6f3 g2f3
    ///  19    6370187   0.29  5.7 1. e2e4 e7e5 g1f3 b8c6 f1b5 a7a6 b5c6 d7c6 d2d3 c8g4 b1d2 g8f6 h2h3 g4f3 d2f3 f8d6 e1g1 e8g8 c1e3 f6d7 d3d4 d8e7
    ///  20    8647917   0.32  7.8 1. e2e4 e7e5 g1f3 b8c6 f1b5 g8e7 b1c3 a7a6 b5c4 e7g6 d2d4 e5d4 f3d4 g6e5 d4c6 d7c6 d1d8 e8d8 c4e2 c8e6 c1f4
    ///     
    ///  Nodes: 8853170  Time spent: 8.01  Nodes/Sec= 1104713
    ///     
    ///  move e2e4
    /// </code>
    /// 
    /// Observe que Tucano atinge a profundidade 20, e tem um formato melhor. Mas existem melhores motores! 
    /// Talvez um dia, Tucano possa ser mais forte, ou talvez o seu futuro motor ;)
    /// 
    /// </remarks>
    public class XBoard
    {
        /// <summary>
        /// Motor de xadrez.
        /// </summary>
        public Motor Motor;

        /// <summary>
        /// Profundidade máxima da pesquisa.
        /// </summary>
        public int ProfundidadeMaxima = Defs.PROFUNDIDADE_MAXIMA;

        /// <summary>
        /// O tempo máximo da pesquisa em milissegundos.
        /// </summary>
        public int MilisegundosPorMovimento = 10000; // 10 segundos
        
        /// <summary>
        /// Cor do jogador associado ao computador.
        /// </summary>
        private sbyte CorComputador = Defs.COR_NENHUMA;

        /// <summary>
        /// O tempo máximo da pesquisa em milissegundos.
        /// </summary>
        /// <param name="motor">Motor a ser controlado pela interface XBoard.</param>
        public XBoard(Motor motor)
        {
            Debug.Assert(motor != null);
            Motor = motor;
        }

        /// <summary>
        /// Loop principal da interface XBoard.
        /// </summary>
        /// <remarks>
        /// Lê comandos da linha de comando e envia para o motor.
        /// Lê e interpreta as respostas do motor.
        /// 
        /// A descrição completa do protocolo XBoard pode ser encontrada em:
        ///     https://www.gnu.org/software/xboard/engine-intf.html
        /// </remarks>
        public void LoopPrincipal()
        {
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

        /// <summary>
        /// Solicita ao motor para procurar o melhor movimento e o executa no tabuleiro.
        /// </summary>
        private void ProcuraMovimento()
        {
            Motor.Pesquisa.LoopAprofundamentoIterativo(MilisegundosPorMovimento, ProfundidadeMaxima);
            if (Motor.Pesquisa.MelhorMovimento != null) {
                Motor.Tabuleiro.FazMovimento(Motor.Pesquisa.MelhorMovimento);
                Console.WriteLine("move " + Motor.Pesquisa.MelhorMovimento.Notacao());
                Console.Out.Flush();
            }
            else {
                CorComputador = Defs.COR_NENHUMA;
            }
        }

        /// <summary>
        /// Verifica se temos um movimento válido e faz o movimento no tabuleiro de xadrez.
        /// </summary>
        /// <param name="linha">Movimento</param>
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

        /// <summary>
        /// Inicia um novo jogo.
        /// </summary>
        private void Comando_new()
        {
            Motor.Tabuleiro.NovaPartida(Defs.FEN_POSICAO_INICIAL);
            CorComputador = Defs.COR_NENHUMA;
        }

        /// <summary>
        /// Interpreta o comando "st": define o tempo.
        /// </summary>
        /// <param name="linha">Comando no formato "st tempo".</param>
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

        /// <summary>
        /// Executa o comando "sd": define a profundidade.
        /// </summary>
        /// <param name="linha">Comando no formato "sd profundidade".</param>
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

        /// <summary>
        /// Executa o comando "time": define o tempo total para a partida.
        /// </summary>
        /// <param name="linha">Comando no formato "time tempo"</param>
        private void Comando_time(string linha)
        {
            try {
                int tempo_total = int.Parse(linha.Substring("time ".Length));
                // Aqui temos o tempo total para o jogo. 
                // Esta é uma estratégia de alocação de tempo bem simples. 
                // Assume que temos 30 movimentos restantes.
                // Você deve dedicar muito tempo, para construir sua estratégia de alocação de tempo :)
                MilisegundosPorMovimento = tempo_total * 10 / 30;
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }
        }
    }
}

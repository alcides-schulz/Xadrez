using System;
using System.Linq;
using System.Diagnostics;

namespace Enxadrista
{
    /// <summary>
    /// Definições Gerais do jogo.
    /// </summary>
    /// <remarks>
    /// Algumas definições precisam estar em harmonia com o tabuleiro, pois dependem
    /// da estrutura (número de colunas vs número de fileiras).
    /// </remarks>
    public class Defs
    {
        /// <summary>
        /// FEN da posição inicial.
        /// </summary>
        public const string FEN_POSICAO_INICIAL = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        // Estrutura do tabuleiro
        public const int PRIMEIRA_FILEIRA = 2;
        public const int ULTIMA_FILEIRA = 10;
        public const int PRIMEIRA_COLUNA = 2;
        public const int ULTIMA_COLUNA = 10;
        public const int NUMERO_FILEIRAS = 12;
        public const int NUMERO_COLUNAS = 12;
        public const int INDICE_MAXIMO = NUMERO_FILEIRAS * NUMERO_COLUNAS;

        // Definições para localizar outras casas no tabuleiro.
        public const int POSICAO_NORTE = -12;
        public const int POSICAO_SUL = 12;
        public const int POSICAO_OESTE = -1;
        public const int POSICAO_LESTE = 1;
        public const int POSICAO_NORDESTE = -11;
        public const int POSICAO_NOROESTE = -13;
        public const int POSICAO_SUDESTE = 13;
        public const int POSICAO_SUDOESTE = 11;

        /// <summary>
        /// Definições de movimentos para peças.
        /// </summary>
        public class Movimentos
        {
            /// <summary>
            /// Movimentos do Cavalo.
            /// </summary>
            public static readonly int[] CAVALO = {
                Defs.POSICAO_NORTE + Defs.POSICAO_NORDESTE,
                Defs.POSICAO_NORTE + Defs.POSICAO_NOROESTE,
                Defs.POSICAO_LESTE + Defs.POSICAO_NORDESTE,
                Defs.POSICAO_LESTE + Defs.POSICAO_SUDESTE,
                Defs.POSICAO_SUL + Defs.POSICAO_SUDESTE,
                Defs.POSICAO_SUL + Defs.POSICAO_SUDOESTE,
                Defs.POSICAO_OESTE + Defs.POSICAO_SUDOESTE,
                Defs.POSICAO_OESTE + Defs.POSICAO_NOROESTE
            };
            /// <summary>
            /// Movimentos do Rei.
            /// </summary>
            public static readonly int[] REI = {
                Defs.POSICAO_NORTE,
                Defs.POSICAO_NORDESTE,
                Defs.POSICAO_LESTE,
                Defs.POSICAO_SUDESTE,
                Defs.POSICAO_SUL,
                Defs.POSICAO_SUDOESTE,
                Defs.POSICAO_OESTE,
                Defs.POSICAO_NOROESTE
            };
            /// <summary>
            /// Movimentos da Torre.
            /// </summary>
            public static readonly int[] TORRE = {
                Defs.POSICAO_NORTE,
                Defs.POSICAO_LESTE,
                Defs.POSICAO_SUL,
                Defs.POSICAO_OESTE
            };
            /// <summary>
            /// Movimentos do Bispo.
            /// </summary>
            public static readonly int[] BISPO = {
                Defs.POSICAO_NORDESTE,
                Defs.POSICAO_SUDESTE,
                Defs.POSICAO_SUDOESTE,
                Defs.POSICAO_NOROESTE
            };
        }

        public const sbyte BORDA = 10;          // Border
        public const sbyte CASA_VAZIA = 0;      // Empty Square

        // Definições das peças para o tabuleiro.
        public const sbyte PEAO_BRANCO = 1;     // White Pawn
        public const sbyte PEAO_PRETO = -1;     // Black Pawn
        public const sbyte CAVALO_BRANCO = 2;   // White Knight
        public const sbyte CAVALO_PRETO = -2;   // Black Knight
        public const sbyte BISPO_BRANCO = 3;    // White Bishop
        public const sbyte BISPO_PRETO = -3;    // Black Bishop
        public const sbyte TORRE_BRANCA = 4;    // White Rook
        public const sbyte TORRE_PRETA = -4;    // Black Rook
        public const sbyte DAMA_BRANCA = 5;     // White Queen
        public const sbyte DAMA_PRETA = -5;     // Black Queen
        public const sbyte REI_BRANCO = 6;      // White King
        public const sbyte REI_PRETO = -6;      // Black King

        public const sbyte PECA_NENHUMA = 0;

        /// <summary>
        /// Coordenadas das posições do tabuleiro para imprimir.
        /// </summary>
        public static readonly string[] COORDENADAS = {
            "", "",   "",   "",   "",   "",   "",   "",  "",   "",  "", "",
            "", "",   "",   "",   "",   "",   "",   "",  "",   "",  "", "",
            "", "", "A8", "B8", "C8", "D8", "E8", "F8", "G8", "H8", "", "",
            "", "", "A7", "B7", "C7", "D7", "E7", "F7", "G7", "H7", "", "",
            "", "", "A6", "B6", "C6", "D6", "E6", "F6", "G6", "H6", "", "",
            "", "", "A5", "B5", "C5", "D5", "E5", "F5", "G5", "H5", "", "",
            "", "", "A4", "B4", "C4", "D4", "E4", "F4", "G4", "H4", "", "",
            "", "", "A3", "B3", "C3", "D3", "E3", "F3", "G3", "H3", "", "",
            "", "", "A2", "B2", "C2", "D2", "E2", "F2", "G2", "H2", "", "",
            "", "", "A1", "B1", "C1", "D1", "E1", "F1", "G1", "H1", "", "",
            "", "",   "",   "",   "",   "",   "",   "",   "",   "", "", "",
            "", "",   "",   "",   "",   "",   "",   "",   "",   "", "", "",
        };

        /// <summary>
        /// Mapeamento de cada casa com a estrutura interna do tabuleiro.
        /// </summary>
        public enum INDICE {
            A8 = (PRIMEIRA_FILEIRA + 0) * NUMERO_COLUNAS + PRIMEIRA_COLUNA, B8, C8, D8, E8, F8, G8, H8,
            A7 = (PRIMEIRA_FILEIRA + 1) * NUMERO_COLUNAS + PRIMEIRA_COLUNA, B7, C7, D7, E7, F7, G7, H7,
            A6 = (PRIMEIRA_FILEIRA + 2) * NUMERO_COLUNAS + PRIMEIRA_COLUNA, B6, C6, D6, E6, F6, G6, H6,
            A5 = (PRIMEIRA_FILEIRA + 3) * NUMERO_COLUNAS + PRIMEIRA_COLUNA, B5, C5, D5, E5, F5, G5, H5,
            A4 = (PRIMEIRA_FILEIRA + 4) * NUMERO_COLUNAS + PRIMEIRA_COLUNA, B4, C4, D4, E4, F4, G4, H4,
            A3 = (PRIMEIRA_FILEIRA + 5) * NUMERO_COLUNAS + PRIMEIRA_COLUNA, B3, C3, D3, E3, F3, G3, H3,
            A2 = (PRIMEIRA_FILEIRA + 6) * NUMERO_COLUNAS + PRIMEIRA_COLUNA, B2, C2, D2, E2, F2, G2, H2,
            A1 = (PRIMEIRA_FILEIRA + 7) * NUMERO_COLUNAS + PRIMEIRA_COLUNA, B1, C1, D1, E1, F1, G1, H1,
        };

        /// <summary>
        /// Número máximo de movimentos que podemos executar durante a pesquisa.
        /// </summary>
        public const int NIVEL_MAXIMO = 128;

        /// <summary>
        /// Número máximo de movimentos que podemos executar para um jogo completo.
        /// </summary>
        public const int HISTORIA_MAXIMA = 1024;

        /// <summary>
        /// Valor limite mínimo para a pesquisa.
        /// </summary>
        public const int VALOR_MINIMO = -32767;

        /// <summary>
        /// Valor limite máximo para a pesquisa.
        /// </summary>
        public const int VALOR_MAXIMO = 32767;

        /// <summary>
        /// Valor do xeque mate.
        /// </summary>
        public const int VALOR_MATE = 30000;

        /// <summary>
        /// valor mínimo de avaliação.
        /// </summary>
        public const int AVALIACAO_MINIMA = -10000;

        /// <summary>
        /// Valor máximo da avaliação.
        /// </summary>
        public const int AVALIACAO_MAXIMA = 10000;

        /// <summary>
        /// Profundidade máxima para a procura.
        /// </summary>
        public const int PROFUNDIDADE_MAXIMA = 64;
        
        /// <summary>
        /// Letra do elemento do tabuleiro.
        /// </summary>
        /// <param name="elemento_do_tabuleiro"></param>
        /// <returns></returns>
        public static char Letra(sbyte elemento_do_tabuleiro)
        {
            if (elemento_do_tabuleiro == BORDA) return '-';

            int valor = Math.Abs(elemento_do_tabuleiro);
            if (valor >= 0 && valor <= 6)
            {
                var letra = " PNBRQK".ElementAt(valor);
                if (elemento_do_tabuleiro < 0) letra = Char.ToLower(letra);
                return letra;
            }
                        
            return '?'; // nao deveria chegar aqui.
        }


        /// <summary>
        /// Conversão de coordenadas.
        /// </summary>
        /// <param name="indice">Índice de coordenada 8x8.</param>
        /// <returns>Índice de coordenada 12x12</returns>
        public static int Converte8x8Para12x12(int indice)
        {
            Debug.Assert(indice >= 0 && indice <= Defs.INDICE_MAXIMO);

            int fileira = indice / Defs.NUMERO_FILEIRAS;
            int coluna = indice - (fileira * Defs.NUMERO_FILEIRAS);

            return (fileira - Defs.PRIMEIRA_FILEIRA) * 8 + coluna - Defs.PRIMEIRA_COLUNA;
        }

        /// <summary>
        /// Conversão de coordenadas.
        /// </summary>
        /// <param name="indice">Índice de coordenada 12x12.</param>
        /// <returns>Índice de coordenada 8x8</returns>
        public static int Converte12x12Para8x8(int indice)
        {
            Debug.Assert(indice >= 0 && indice <= Defs.INDICE_MAXIMO);

            int fileira = indice / Defs.NUMERO_FILEIRAS;
            int coluna = indice - (fileira * Defs.NUMERO_FILEIRAS);

            return (fileira - Defs.PRIMEIRA_FILEIRA) * 8 + coluna - Defs.PRIMEIRA_COLUNA;
        }

        /// <summary>
        /// Obtem o índice do quadrado a partir da descrição de posição.
        /// Cada casa é representada por uma dessas posições:
        ///     A8, B8, C8, D8, E8, F8, G8, H8
        ///     A7, B7, C7, D7, E7, F7, G7, H7
        ///     A6, B6, C6, D6, E6, F6, G6, H6
        ///     A5, B5, C5, D5, E5, F5, G5, H5
        ///     A4, B4, C4, D4, E4, F4, G4, H4
        ///     A3, B3, C3, D3, E3, F3, G3, H3
        ///     A2, B2, C2, D2, E2, F2, G2, H2
        ///     A1, B1, C1, D1, E1, F1, G1, H1
        /// </summary>
        /// <param name="posicao">Letra e número da posição</param>
        /// <returns>Índice da posição</returns>
        public static int ObtemIndiceDaPosicao(string posicao)
        {
            if (posicao.Length != 2) return 0;

            int numero_coluna = "abcdefgh".IndexOf(Char.ToLower(posicao[0]));
            int numero_fileira = "87654321".IndexOf(Char.ToLower(posicao[1]));

            if (numero_coluna == -1 || numero_fileira == -1) return 0;

            return (numero_fileira + Defs.PRIMEIRA_FILEIRA) * Defs.NUMERO_COLUNAS + (numero_coluna + Defs.PRIMEIRA_COLUNA);
        }

        /// <summary>
        /// Retorna a letra e o número do índice de posição no tabuleiro de xadrez.
        /// </summary>
        /// <param name="indice">Índice da casa</param>
        /// <returns>A8, B8, C8, etc.</returns>
        public static string ObtemDescCasa(int indice)
        {
            return (indice >= (int)Defs.INDICE.A8 && indice <= (int)Defs.INDICE.H1) ? Defs.COORDENADAS[indice].ToLower() : "-";
        }

        /// <summary>
        /// Indica se o índice está na fileira de promoção branca (A8 a H8).
        /// </summary>
        /// <param name="indice">Índice da casa.</param>
        /// <returns>Verdadeiro para índice entre A8 e H8.</returns>
        public static bool FileiraPromocaoBranco(int indice)
        {
            return indice >= (int)Defs.INDICE.A8 && indice <= (int)Defs.INDICE.H8;
        }

        /// <summary>
        /// Indica se o índice está na fileira de promoção preta (A1 a H1).
        /// </summary>
        /// <param name="indice">Índice da casa.</param>
        /// <returns>Verdadeiro para índice entre A1 e H1.</returns>
        public static bool FileiraPromocaoPreto(int indice)
        {
            return indice >= (int)Defs.INDICE.A1 && indice <= (int)Defs.INDICE.H1;
        }

        /// <summary>
        /// Indica se o índice está na primeira fileira peões brancos (A2 a H2).
        /// </summary>
        /// <param name="indice">Índice da casa.</param>
        /// <returns>Verdadeiro para índice entre A2 e H2.</returns>
        public static bool PrimeiraFileiraPeaoBranco(int indice)
        {
            return indice >= (int)Defs.INDICE.A2 && indice <= (int)Defs.INDICE.H2;
        }

        /// <summary>
        /// Indica se o índice está na primeira fileira de peões pretos (A7 a H7).
        /// </summary>
        /// <param name="indice">Índice da casa.</param>
        /// <returns>Verdadeiro para índice entre A7 e H7.</returns>
        public static bool PrimeiraFileiraPeaoPreto(int indice)
        {
            return indice >= (int)Defs.INDICE.A7 && indice <= (int)Defs.INDICE.H7;
        }

    }
}

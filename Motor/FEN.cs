using System;

namespace Enxadrista
{
    /// <summary>
    /// FEN é um padrão para representar uma posição do jogo.
    /// </summary>
    /// <remarks>
    /// 
    /// https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation
    /// 
    /// A descrição FEN é em inglês. As peças sao representadas assim:
    /// 
    ///    Peão = Pawn(P)
    ///    Cavalo = Knight(N)
    ///    Bispo = Bishop(B)
    ///    Torre = Rook(R)
    ///    Dama = Queen(Q)
    ///    Rei = King(K)
    ///
    /// Por exemplo: a posição inicial é representada por este FEN:
    ///    rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
    ///    
    /// A vantagem de usar FEN é que este é um padrão usado pela maioria dos softwares de xadrez.
    /// 
    /// </remarks>
    public class FEN
    {
        /// <summary>
        /// Converte uma string FEN para o tabuleiro.
        /// </summary>
        /// <param name="fen">string FEN representando uma posicão</param>
        /// <param name="tabuleiro">tabuleiro a ser atualizado com a posicão FEN</param>
        public static void ConverteFenParaTabuleiro(string fen, Tabuleiro tabuleiro)
        {
            string[] partes = fen.Split(' ');

            string posicao_pecas = partes[0];
            string cor_jogar = partes[1];
            string estado_roque = partes[2];
            string casa_en_passant = partes[3];
            string contador_regra_50 = partes.Length > 4 ? partes[4] : "0";
            string contador_movimentos = partes.Length > 4 ? partes[5] : "0";

            int fileira = Defs.PRIMEIRA_FILEIRA;
            int coluna = Defs.PRIMEIRA_COLUNA;

            foreach (char item in posicao_pecas) {
                switch (item) {
                    case 'r': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.TORRE_PRETA); break;
                    case 'n': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.CAVALO_PRETO); break;
                    case 'b': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.BISPO_PRETO); break;
                    case 'q': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.DAMA_PRETA); break;
                    case 'k': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.REI_PRETO); break;
                    case 'p': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.PEAO_PRETO); break;
                    case 'R': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.TORRE_BRANCA); break;
                    case 'N': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.CAVALO_BRANCO); break;
                    case 'B': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.BISPO_BRANCO); break;
                    case 'Q': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.DAMA_BRANCA); break;
                    case 'K': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.REI_BRANCO); break;
                    case 'P': tabuleiro.ColocaPeca(fileira * Defs.NUMERO_COLUNAS + coluna++, Defs.PEAO_BRANCO); break;
                    case '/': fileira += 1; coluna = Defs.PRIMEIRA_COLUNA; break;
                    default: if (Char.IsDigit(item)) coluna += int.Parse(item.ToString()); break;
                }
            }

            tabuleiro.CorJogar = cor_jogar.ParaCor();

            if (estado_roque.Contains('K')) tabuleiro.RoqueE1G1 = true;
            if (estado_roque.Contains('Q')) tabuleiro.RoqueE1C1 = true;
            if (estado_roque.Contains('k')) tabuleiro.RoqueE8G8 = true;
            if (estado_roque.Contains('q')) tabuleiro.RoqueE8C8 = true;

            tabuleiro.IndiceEnPassant = Defs.ObtemIndiceDaPosicao(casa_en_passant);

            tabuleiro.ContadorRegra50 = int.Parse(contador_regra_50);

            tabuleiro.ContadorMovimentos = int.Parse(contador_movimentos);
        }


        /// <summary>
        /// Gera uma string FEN representando a posição atual no tabuleiro .
        /// </summary>
        /// <param name="tabuleiro">tabuleiro com a posição para gerar a string FEN</param>
        /// <returns>string FEN</returns>
        public static string ConverteTabuleiroParaFEN(Tabuleiro tabuleiro)
        {
            string fen = "";

            fen += FEN.ObtemDescPosicao(tabuleiro);
            fen += " " + tabuleiro.CorJogar.ParaTexto();
            fen += " " + FEN.ObtemDescRoque(tabuleiro);
            fen += " " + Defs.ObtemDescCasa(tabuleiro.IndiceEnPassant);
            fen += " " + tabuleiro.ContadorRegra50.ToString();
            fen += " " + tabuleiro.ContadorMovimentos.ToString();

            return fen;
        }

        /// <summary>
        /// Retorna a representação FEN atual das peças no tabuleiro de xadrez.
        /// Exemplo para posição inicial: rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR
        /// </summary>
        /// <param name="tabuleiro">tabuleiro com a posição para gerar a string FEN</param>
        /// <returns>Descrição FEN das peças no tabuleiro</returns>
        private static string ObtemDescPosicao(Tabuleiro tabuleiro)
        {
            var desc = "";

            for (int fileira = Defs.PRIMEIRA_FILEIRA; fileira < Defs.ULTIMA_FILEIRA; fileira++) {
                if (desc != "") desc += "/";
                int contador_casas_vazias = 0;
                for (int coluna = Defs.PRIMEIRA_COLUNA; coluna < Defs.ULTIMA_COLUNA; coluna++) {
                    sbyte peca = tabuleiro.ObtemPeca(fileira * Defs.NUMERO_COLUNAS + coluna);
                    if (peca == Defs.CASA_VAZIA) {
                        contador_casas_vazias++;
                        continue;
                    }
                    if (contador_casas_vazias != 0) {
                        desc += contador_casas_vazias.ToString();
                        contador_casas_vazias = 0;
                    }
                    desc += Defs.Letra(peca);
                }
                if (contador_casas_vazias != 0) {
                    desc += contador_casas_vazias.ToString();
                    contador_casas_vazias = 0;
                }
            }

            return desc;
        }

        /// <summary>
        /// Retorna as letras que representam a possibilidade do roque para a string FEN.
        /// K Roque pequeno branco
        /// Q Roque grande branco
        /// k Roque pequeno preto
        /// q Roque grande preto
        /// </summary>
        /// <param name="tabuleiro">tabuleiro com a posição para gerar a string FEN</param>
        /// <returns>Letras KQkq de acordo com o estado do roque</returns>
        private static string ObtemDescRoque(Tabuleiro tabuleiro)
        {
            string roque = "";

            if (tabuleiro.RoqueE1G1) roque += "K";
            if (tabuleiro.RoqueE1C1) roque += "Q";
            if (tabuleiro.RoqueE8G8) roque += "k";
            if (tabuleiro.RoqueE8C8) roque += "q";

            return roque == "" ? "-" : roque;
        }
    }
}

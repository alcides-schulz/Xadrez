using System.Diagnostics;

namespace Enxadrista
{
    /// <summary>
    ///     Define tipos de peça com todas os tipos presentes no jogo.
    /// </summary>
    public enum TipoPeca : sbyte
    {
        Nenhum = 0,
        Peao = 1,
        Cavalo = 2,
        Bispo = 3,
        Torre = 4,
        Dama = 5,
        Rei = 6
    }

    /// <summary>
    ///     Define a peça com a sua respectiva cor.
    /// </summary>
    public enum Peca : sbyte
    {
        Borda = 0,
        Nenhuma = 1,
        PeaoBranco = 2,
        CavaloBranco = 3,
        BispoBranco = 4,
        TorreBranca = 5,
        DamaBranca = 6,
        ReiBranco = 7,
        PeaoPreto = 10,
        CavaloPreto = 11,
        BispoPreto = 12,
        TorrePreta = 13,
        DamaPreta = 14,
        ReiPreto = 15
    }

    /// <summary>
    ///     Classe auxiliar com funções de pecas.
    /// </summary>
    public static class PecaExtensao
    {
        /// <summary>
        ///     Transforma a peça em uma lista de indices válidos entre 0 e a quantidade total de peças (12).
        /// </summary>
        public static int ParaIndice(this Peca peca)
        {
            return (int) peca - 2 * (1 + (int) peca.ParaCor());
        }

        /// <summary>
        ///     Transforma peça em tipo de peça.
        /// </summary>
        public static TipoPeca ParaTipo(this Peca peca)
        {
            return (TipoPeca) (((int) peca & 0x7) - 1);
        }

        /// <summary>
        ///     Transforma peça em representação de texto.
        /// </summary>
        public static string ParaTexto(this Peca peca)
        {
            var tipos = new[] {"-", "-", "P", "N", "B", "R", "Q", "K", "-", "-", "p", "n", "b", "r", "q", "k"};
            return tipos[(int) peca];
        }

        /// <summary>
        ///     Transforma um caractere em uma Peça.
        /// </summary>
        public static Peca ParaPeca(this char peca)
        {
            switch (peca)
            {
                case 'P': return Peca.PeaoBranco;
                case 'B': return Peca.BispoBranco;
                case 'N': return Peca.CavaloBranco;
                case 'R': return Peca.TorreBranca;
                case 'Q': return Peca.DamaBranca;
                case 'K': return Peca.ReiBranco;
                case 'n': return Peca.CavaloPreto;
                case 'b': return Peca.BispoPreto;
                case 'p': return Peca.PeaoPreto;
                case 'r': return Peca.TorrePreta;
                case 'q': return Peca.DamaPreta;
                case 'k': return Peca.ReiPreto;
                default: return Peca.Nenhuma;
            }
        }

        /// <summary>
        ///     Calcula a cor de uma peça.
        /// </summary>
        public static Cor ParaCor(this Peca peca)
        {
            switch (peca)
            {
                case Peca.PeaoBranco:
                case Peca.BispoBranco:
                case Peca.CavaloBranco:
                case Peca.TorreBranca:
                case Peca.DamaBranca:
                case Peca.ReiBranco:
                    return Cor.Branca;
                case Peca.CavaloPreto:
                case Peca.BispoPreto: 
                case Peca.PeaoPreto: 
                case Peca.TorrePreta: 
                case Peca.DamaPreta:
                case Peca.ReiPreto:
                    return Cor.Preta;
                default: return Cor.Nenhuma;
            }
        }

        /// <summary>
        ///     Inverte a cor de uma peça.
        /// </summary>
        public static Peca InverteCor(this Peca peca)
        {
            Debug.Assert(peca != Peca.Borda && peca != Peca.Nenhuma);
            return (Peca) ((int) peca ^ 0x8);
        }

        /// <summary>
        ///     Inverte a cor de uma peça.
        /// </summary>
        public static Peca ParaPeca(this TipoPeca tipoPeca, Cor cor)
        {
            return (Peca) (((int) tipoPeca + 1) ^ ((int) cor * 0x8));
        }
    }
}
namespace Enxadrista
{
    /// <summary>
    /// Movimento.
    /// </summary>
    /// <remarks>
    /// Salva informações sobre movimentos de xadrez.
    /// </remarks>
    public class Movimento
    {
        /// <summary>
        /// Peça que está se movendo.
        /// </summary>
        public sbyte Peca = Defs.PECA_NENHUMA;
        /// <summary>
        /// Índice da casa de origem.
        /// </summary>
        public int IndiceOrigem = 0;
        /// <summary>
        /// Índice da casa de destino.
        /// </summary>
        public int IndiceDestino = 0;
        /// <summary>
        /// Peça sendo capturada para movimentos de captura.
        /// </summary>
        public sbyte PecaCaptura = Defs.PECA_NENHUMA;
        /// <summary>
        /// Peça para movimentos de promoção.
        /// </summary>
        public sbyte PecaPromocao = Defs.PECA_NENHUMA;
        /// <summary>
        /// Índice da casa do peão sendo removido em capturas ep-passant.
        /// </summary>
        public int IndicePeaoEnPassant = 0;
        /// <summary>
        /// Valor usado pela classificação de movimento durante a pesquisa.
        /// </summary>
        public int ValorOrdenacao;

        /// <summary>
        /// Cria um movimento simples.
        /// </summary>
        /// <param name="peca">Peça que está se movendo.</param>
        /// <param name="indice_origem">Índice da casa de origem.</param>
        /// <param name="indice_destino">Índice da casa de destino.</param>
        public Movimento(sbyte peca, int indice_origem, int indice_destino)
        {
            Peca = peca;
            IndiceOrigem = indice_origem;
            IndiceDestino = indice_destino;
            ValorOrdenacao = 0;
        }

        /// <summary>
        /// Cria um movimento de captura.
        /// </summary>
        /// <param name="peca">Peça que está se movendo.</param>
        /// <param name="indice_origem">Índice da casa de origem.</param>
        /// <param name="indice_destino">Índice da casa de destino.</param>
        /// <param name="peca_captura">Peça capturada</param>
        public Movimento(sbyte peca, int indice_origem, int indice_destino, sbyte peca_captura) : 
            this(peca, indice_origem, indice_destino)
        {
            PecaCaptura = peca_captura;
        }

        /// <summary>
        /// Cria um movimento de promoção ou captura/promoção.
        /// </summary>
        /// <param name="peca">Peça que está se movendo.</param>
        /// <param name="indice_origem">Índice da casa de origem.</param>
        /// <param name="indice_destino">Índice da casa de destino.</param>
        /// <param name="peca_captura">Peça sendo capturada</param>
        /// <param name="peca_promocao">Peça de promoção (Dama, Torre, Bispo ou Cavalo)</param>
        public Movimento(sbyte peca, int indice_origem, int indice_destino, sbyte peca_captura, sbyte peca_promocao) :
            this(peca, indice_origem, indice_destino)
        {
            PecaCaptura = peca_captura;
            PecaPromocao = peca_promocao;
        }

        /// <summary>
        /// Captura de peão en-passant.
        /// </summary>
        /// <param name="peca">Peão</param>
        /// <param name="indice_origem">Índice da casa de origem.</param>
        /// <param name="indice_destino">Índice da casa de destino.</param>
        /// <param name="indice_peao_enpassant">Índice da casa do peão sendo removido.</param>
        public Movimento(sbyte peca, int indice_origem, int indice_destino, int indice_peao_enpassant) :
            this(peca, indice_origem, indice_destino)
        {
            IndicePeaoEnPassant = indice_peao_enpassant;
        }

        /// <summary>
        /// Indica se o movimento é uma captura.
        /// </summary>
        /// <returns>Verdadeiro para capturas.</returns>
        public bool Captura()
        {
            return PecaCaptura != Defs.PECA_NENHUMA;
        }

        /// <summary>
        /// Indica se o movimento é uma promoção.
        /// </summary>
        /// <returns>Verdadeiro para promoção.</returns>
        public bool Promocao()
        {
            return PecaPromocao != Defs.PECA_NENHUMA;
        }

        /// <summary>
        /// Indica que é uma jogada tática: captura ou promoção.
        /// </summary>
        /// <returns>Verdadeiro para captura ou promoção.</returns>
        public bool Tatico()
        {
            return Captura() || Promocao();
        }

        /// <summary>
        /// Indica que é uma captura e promoção (peão captura uma peça na fileira de promoção).
        /// </summary>
        /// <returns>Verdadeiro para captura e promoção.</returns>
        public bool CapturaPromocao()
        {
            return PecaPromocao != Defs.PECA_NENHUMA && PecaCaptura != Defs.PECA_NENHUMA;
        }

        /// <summary>
        /// Indica se este movimento é roque E1G1.
        /// </summary>
        /// <returns>Verdadeiro para o movimento E1G1.</returns>
        public bool RoqueE1G1()
        {
            return Peca == Defs.REI_BRANCO && IndiceOrigem == (int)Defs.INDICE.E1 && IndiceDestino == (int)Defs.INDICE.G1;
        }

        /// <summary>
        /// Indica se este movimento é roque E1C1.
        /// </summary>
        /// <returns>Verdadeiro para o movimento E1C1.</returns>
        public bool RoqueE1C1()
        {
            return Peca == Defs.REI_BRANCO && IndiceOrigem == (int)Defs.INDICE.E1 && IndiceDestino == (int)Defs.INDICE.C1;
        }

        /// <summary>
        /// Indica se este movimento é roque E8G8.
        /// </summary>
        /// <returns>Verdadeiro para o movimento E8G8.</returns>
        public bool RoqueE8G8()
        {
            return Peca == Defs.REI_PRETO && IndiceOrigem == (int)Defs.INDICE.E8 && IndiceDestino == (int)Defs.INDICE.G8;
        }

        /// <summary>
        /// Indica se este movimento é roque E8C8.
        /// </summary>
        /// <returns>Verdadeiro para o movimento E8C8.</returns>
        public bool RoqueE8C8()
        {
            return Peca == Defs.REI_PRETO && IndiceOrigem == (int)Defs.INDICE.E8 && IndiceDestino == (int)Defs.INDICE.C8;
        }

        /// <summary>
        /// Notação do movimento.
        /// </summary>
        /// <returns>String representando a notação do movimento.</returns>
        public string Notacao()
        {
            var s = "";

            s += Defs.COORDENADAS[IndiceOrigem];
            s += Defs.COORDENADAS[IndiceDestino];
            if (Promocao()) s += Defs.Letra(PecaPromocao);

            return s.ToLower();
        }

        /// <summary>
        /// Compara dois movimentos para indicar se eles são os mesmos, ou seja, mesmas peças e coordenadas. 
        /// </summary>
        /// <param name="objeto">Outro objeto de movimento a ser comparado.</param>
        /// <returns>Verdadeiro se o objeto é igual a este movimento.</returns>
        public override bool Equals(object objeto)
        {
            if (objeto == null) return false;
            if (objeto.GetType() != this.GetType()) return false;
            Movimento outro = (Movimento)objeto;
            if (this.IndiceOrigem == outro.IndiceOrigem && this.IndiceDestino == outro.IndiceDestino) {
                if (this.Peca == outro.Peca && this.PecaCaptura == outro.PecaCaptura && this.PecaPromocao == outro.PecaPromocao) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Cria o código hash para este movimento. 
        /// </summary>
        /// <remarks>
        /// Recomendado quando você substitui Equals. O compilador dá esse aviso. Não relacionado ao motor de xadrez.
        /// </remarks>
        /// <returns>Código hash</returns>
        public override int GetHashCode()
        {
            int hash_code = base.GetHashCode();
            hash_code ^= IndiceOrigem;
            hash_code ^= IndiceDestino;
            hash_code ^= Peca;
            hash_code ^= PecaCaptura;
            hash_code ^= PecaPromocao;
            return hash_code;
        }

        /// <summary>
        /// Representação deste movimento.
        /// </summary>
        /// <returns>String com a representação deste mmovimento.</returns>
        public override string ToString()
        {
            var s = "";

            s += Defs.COORDENADAS[IndiceOrigem];
            s += Captura() ? "x" : "-";
            s += Defs.COORDENADAS[IndiceDestino];
            if (Promocao()) s += Defs.Letra(PecaPromocao);

            return s.ToLower();
        }
    }
}

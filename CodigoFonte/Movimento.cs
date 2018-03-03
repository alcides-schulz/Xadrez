using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enxadrista
{
    public class Movimento
    {
        public sbyte Peca = Defs.PECA_NENHUMA;
        public int IndiceOrigem = 0;
        public int IndiceDestino = 0;
        public sbyte PecaCaptura = Defs.PECA_NENHUMA;
        public sbyte PecaPromocao = Defs.PECA_NENHUMA;
        public int IndicePeaoEnPassant = 0;
        public int ValorOrdenacao;

        public Movimento(sbyte peca, int indice_origem, int indice_destino)
        {
            Peca = peca;
            IndiceOrigem = indice_origem;
            IndiceDestino = indice_destino;
        }

        public Movimento(sbyte peca, int indice_origem, int indice_destino, sbyte peca_captura) : 
            this(peca, indice_origem, indice_destino)
        {
            PecaCaptura = peca_captura;
        }

        public Movimento(sbyte peca, int indice_origem, int indice_destino, sbyte peca_captura, sbyte peca_promocao) :
            this(peca, indice_origem, indice_destino)
        {
            PecaCaptura = peca_captura;
            PecaPromocao = peca_promocao;
        }

        public Movimento(sbyte peca, int indice_origem, int indice_destino, int indice_peao_enpassant) :
            this(peca, indice_origem, indice_destino)
        {
            IndicePeaoEnPassant = indice_peao_enpassant;
        }

        public bool Captura()
        {
            return PecaCaptura != Defs.PECA_NENHUMA;
        }

        public bool Promocao()
        {
            return PecaPromocao != Defs.PECA_NENHUMA;
        }

        public bool Tatico()
        {
            return Captura() || Promocao();
        }

        public bool CapturaPromocao()
        {
            return PecaPromocao != Defs.PECA_NENHUMA && PecaCaptura != Defs.PECA_NENHUMA;
        }

        public bool RoqueE1G1()
        {
            return Peca == Defs.REI_BRANCO && IndiceOrigem == (int)Defs.INDICE.E1 && IndiceDestino == (int)Defs.INDICE.G1;
        }

        public bool RoqueE1C1()
        {
            return Peca == Defs.REI_BRANCO && IndiceOrigem == (int)Defs.INDICE.E1 && IndiceDestino == (int)Defs.INDICE.C1;
        }

        public bool RoqueE8G8()
        {
            return Peca == Defs.REI_PRETO && IndiceOrigem == (int)Defs.INDICE.E8 && IndiceDestino == (int)Defs.INDICE.G8;
        }

        public bool RoqueE8C8()
        {
            return Peca == Defs.REI_PRETO && IndiceOrigem == (int)Defs.INDICE.E8 && IndiceDestino == (int)Defs.INDICE.C8;
        }

        public string Notacao()
        {
            var s = "";

            s += Defs.COORDENADAS[IndiceOrigem];
            s += Defs.COORDENADAS[IndiceDestino];
            if (Promocao()) s += Defs.Representacao(PecaPromocao);

            return s.ToLower();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != this.GetType()) return false;
            Movimento outro = (Movimento)obj;
            if (this.IndiceOrigem == outro.IndiceOrigem && this.IndiceDestino == outro.IndiceDestino) {
                if (this.Peca == outro.Peca && this.PecaCaptura == outro.PecaCaptura && this.PecaPromocao == outro.PecaPromocao) {
                    return true;
                }
            }
            return false;
        }

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

        public override string ToString()
        {
            var s = "";

            s += Defs.COORDENADAS[IndiceOrigem];
            s += Captura() ? "x" : "-";
            s += Defs.COORDENADAS[IndiceDestino];
            if (Promocao()) s += Defs.Representacao(PecaPromocao);

            return s.ToLower();
        }
    }
}

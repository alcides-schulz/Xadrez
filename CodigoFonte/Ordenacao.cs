using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Enxadrista
{
    public class Ordenacao
    {
        public const int NUMERO_PECAS = 12;
        public const int NUMERO_CASAS = 64;

        public int[][] Tabela = new int[NUMERO_PECAS][];

        public Ordenacao() {
            for (int peca = 0; peca < NUMERO_PECAS; peca++) {
                Tabela[peca] = new int[NUMERO_CASAS];
            }
            Inicia();
        }

        public void Inicia()
        {
            for (int peca = 0; peca < NUMERO_PECAS; peca++) {
                for (int casa = 0; casa < NUMERO_CASAS; casa++) {
                    Tabela[peca][casa] = 0;
                }
            }
        }

        public void AtualizaHistoria(int cor, Movimento movimento, int profundidade)
        {
            if (movimento.Tatico()) return;

            int indice_peca = IndicePeca(cor, movimento.Peca);
            int indice_casa = Defs.Converte12x12Para8x8(movimento.IndiceDestino);

            Tabela[indice_peca][indice_casa] += profundidade;

            if (Tabela[indice_peca][indice_casa] > 9000) {
                for (int peca = 0; peca < NUMERO_PECAS; peca++) {
                    for (int casa = 0; casa < NUMERO_CASAS; casa++) {
                        Tabela[peca][casa] /= 8;
                    }
                }
            }
          }

        public List<Movimento> Orderna(int cor, List<Movimento> lista, Movimento melhor)
        {
            foreach (var movimento in lista) {
                if (melhor != null && movimento.Equals(melhor)) {
                    movimento.ValorOrdenacao = 100000000;
                    continue;
                }

                if (movimento.Captura()) {
                    movimento.ValorOrdenacao = ValorCaptura(movimento) * 10000;
                    continue;
                }
                int indice_peca = IndicePeca(cor, movimento.Peca);
                int indice_casa = Defs.Converte12x12Para8x8(movimento.IndiceDestino);
                movimento.ValorOrdenacao = Tabela[indice_peca][indice_casa];
            }

            return lista.OrderByDescending(m => m.ValorOrdenacao).ToList();
        }

        private int ValorCaptura(Movimento movimento)
        {
            Debug.Assert(movimento.Captura());

            int tipo_peca_captura = Math.Abs(movimento.PecaCaptura);
            int tipo_peca_atacante = Math.Abs(movimento.Peca);

            int valor = tipo_peca_captura * 6 + 5 - tipo_peca_atacante;
            if (movimento.Promocao()) valor -= 5;

            return valor;
        }

        private int IndicePeca(int cor, sbyte peca)
        {
            int indice = Math.Abs(peca) - 1;
            if (cor == Defs.COR_PRETA) indice += 6;
            Debug.Assert(indice >= 0 && indice < NUMERO_PECAS);
            return indice;
        }

    }
}

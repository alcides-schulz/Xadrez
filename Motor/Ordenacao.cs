using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Enxadrista
{
    /// <summary>
    /// Ordenação dos movimentos para a pesquisa.
    /// </summary>
    /// <remarks>
    /// É muito importante que um motor tenha uma boa classificação de movimento para a pesquisa.
    /// Isso ajudará a pesquisa alpha-beta, já que você terá mais cortes beta. Se você tiver um movimento
    /// muito bom procurado mais cedo, pode causar um corte beta e evitar procurar o resto dos movimentos.
    /// Claro, a classificação dos movimentos é uma estimativa, porque você só vai saber o melhor movimento 
    /// após a pesquisa, uma vez que o objetivo da pesquisa é encontrar o melhor movimento. Podemos dizer que 
    /// temos um paradoxo! 
    /// Normalmente, a ordem dos movimento para pesquisa será:
    /// - movimento da tabela de transposição, este foi definido como bom e gravado na tabela de transposição.
    /// - capturas classificadas pela vítima mais valiosa, atacante menos valioso. Exemplo: geralmente é melhor
    ///   procurar primeiro o peão captura dama, do que dama captura peão.
    /// - movimentos matadores (killer moves).
    /// - movimentos simples usando uma tabela de histórico.
    /// Mais opções podem ser aplicadas aqui, como deixar as capturas perdidas no final.
    /// Outra técnica popular que pode ser aplicada é a chamada SEE, avaliação de troca estática, ou Static Exchange Evaluation.
    /// A Enxadrista não usa SEE, ela pode ser implementada mais tarde, e a maioria dos motores de xadrez
    /// implementa e usa SEE, em diferentes formas e componentes do motor.
    /// </remarks>
    /// <see cref="Pesquisa.AlfaBeta(int, int, int, int, List{Movimento})"/>
    /// <see cref="Transposicao"/>
    public class Ordenacao
    {
        public const int NUMERO_PECAS = 12;
        public const int NUMERO_CASAS = 64;

        /// <summary>
        /// Tabela para manter um valor para cada peça e índice destino.
        /// </summary>
        /// <remarks>
        /// Esta informação é atualizada sempre que há uma boa jogada e será usada para ordenar os movimentos 
        /// mais tarde. Não é uma ciência exata, diferentes programas usam técnicas diferentes, tudo depende do 
        /// que funciona melhor para seu programa e sua preferência.
        /// </remarks>
        public int[][] Tabela = new int[NUMERO_PECAS][];

        /// <summary>
        /// Cria o componente de ordenação de movimentos.
        /// </summary>
        public Ordenacao() {
            for (int peca = 0; peca < NUMERO_PECAS; peca++) {
                Tabela[peca] = new int[NUMERO_CASAS];
            }
            Inicia();
        }

        /// <summary>
        /// Prepara os valores da tabela de ordenação.
        /// </summary>
        public void Inicia()
        {
            for (int peca = 0; peca < NUMERO_PECAS; peca++) {
                for (int casa = 0; casa < NUMERO_CASAS; casa++) {
                    Tabela[peca][casa] = 0;
                }
            }
        }

        /// <summary>
        /// Atualiza informação sempre que um bom movimento é encontrada.
        /// </summary>
        /// <remarks>
        /// Note que consideramos somente os movimentos simples. Captura já possui
        /// um valor maior do que movimentos simples. 
        /// </remarks>
        /// <param name="cor">Cor do lado fazendo o movimento.</param>
        /// <param name="movimento">Movimento considerado bom.</param>
        /// <param name="profundidade">Profundidade que o movimento foi pesquisado.</param>
        public void AtualizaHistoria(Cor cor, Movimento movimento, int profundidade)
        {
            if (movimento.Tatico()) return;

            int indice_peca = IndicePeca(cor, movimento.Peca);
            int indice_casa = Defs.Converte12x12Para8x8(movimento.IndiceDestino);

            Tabela[indice_peca][indice_casa] += profundidade;

            // Se o valor na tabela estiver muito alto, é feito um ajuste a todos valores.
            if (Tabela[indice_peca][indice_casa] > 9000) {
                for (int peca = 0; peca < NUMERO_PECAS; peca++) {
                    for (int casa = 0; casa < NUMERO_CASAS; casa++) {
                        Tabela[peca][casa] /= 8;
                    }
                }
            }
        }

        /// <summary>
        /// Retorna a lista ordenada de movimentos.
        /// </summary>
        /// <remarks>
        /// Ordem de classificação:
        /// 1. Movimento melhor vindo da tabela de transposição.
        /// 2. Capturas
        /// 3. Movimentos simples classificados pela tabela de valores.
        /// 
        /// Falta aqui os movimentos matadores (killer moves). Quase todos os programas de xadrez 
        /// usam esta técnica. Eu me pergunto se alguém poderia fazer isso e se faria o Enxadrista 
        /// um pouco melhor.
        /// </remarks>
        /// <param name="cor">Cor do lado com movimentos a ordenar.</param>
        /// <param name="lista">List de movimentos.</param>
        /// <param name="melhor">Melhor movimento que será ordenado primeiro, normalmente da tabela de transposição</param>
        /// <returns>Lista ordenada de movimentos.</returns>
        public List<Movimento> Orderna(Cor cor, List<Movimento> lista, Movimento melhor)
        {
            foreach (var movimento in lista) {
                if (movimento.Equals(melhor)) {
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

        /// <summary>
        /// Calcula o valor da captura.
        /// </summary>
        /// <remarks>
        /// Calcula um valor baseado no valor da peça atacante e da peça atacada (vítima).
        /// Esta técnica é chamada MVV / LVA (most valuable victim, least valuable attacker).
        /// </remarks>
        /// <param name="movimento">Movimento de captura.</param>
        /// <returns>Valor da captura.</returns>
        private int ValorCaptura(Movimento movimento)
        {
            Debug.Assert(movimento.Captura());

            int tipo_peca_captura = Math.Abs(movimento.PecaCaptura);
            int tipo_peca_atacante = Math.Abs(movimento.Peca);

            int valor = tipo_peca_captura * 6 + 5 - tipo_peca_atacante;
            if (movimento.Promocao()) valor -= 5;

            return valor;
        }

        /// <summary>
        /// Obtém o índice da peça com base na cor e no tipo.
        /// </summary>
        /// <remarks>
        /// Usado para a tabela de valores.
        /// </remarks>
        /// <param name="cor">Cor da peça.</param>
        /// <param name="peca">Tipo da peça.</param>
        /// <returns>Índice da peça.</returns>
        private int IndicePeca(Cor cor, sbyte peca)
        {
            int indice = Math.Abs(peca) - 1;
            if (cor == Cor.Preta) indice += 6;
            Debug.Assert(indice >= 0 && indice < NUMERO_PECAS);
            return indice;
        }

    }
}

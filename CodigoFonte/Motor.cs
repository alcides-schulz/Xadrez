using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enxadrista
{
    /// <summary>
    /// Motor de xadrez.
    /// </summary>
    /// <remarks>
    /// Este é o motor de xadrez. Basicamente, conecta os componentes da lógica 
    /// de xadrez: tabuleiro de xadrez, avaliação e pesquisa. Também conecta a 
    /// tabela de transposição.
    /// A principal idéia é que, no futuro, podemos implementar a busca multi-processo,
    /// conhecido como multi-thread ou SMP Search.
    /// Hoje, isso é muito comum para os motores de xadrez competitivos. Enxadrista é
    /// executado em processo único, mas pode ser modificado para ser executado em
    /// vários processos. Eu planejo fazer isso mais tarde.
    /// </remarks>
    public class Motor
    {
        public Tabuleiro Tabuleiro;
        public Avaliacao Avaliacao;
        public Pesquisa Pesquisa;

        /// <summary>
        /// Cria o motor de xadrez.
        /// </summary>
        /// <remarks>
        /// A maneira como implementamos aqui é preparar uma mudança futura para usar o multi-processo (multi-thread).
        /// </remarks>
        /// <param name="transposicao">Tabela de transposição</param>
        public Motor(Transposicao transposicao)
        {
            Tabuleiro = new Tabuleiro();
            Avaliacao = new Avaliacao(Tabuleiro);
            Pesquisa = new Pesquisa(Tabuleiro, Avaliacao, transposicao);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Enxadrista
{
    /// <summary>
    /// Tabela de Transposição.
    /// </summary>
    /// <remarks>
    /// Esta tabela será usada para armazenar informações sobre as posições visitadas, e 
    /// quando a mesma posição for visitada novamente, as informações armazenadas podem 
    /// ser usadas para economizar tempo.
    /// Normalmente, o usuário irá decidir o tamanho da tabela e pode ser muito grande, 
    /// desde que tenha memória disponível. Este é um componente importante para programas 
    /// competitivos.
    /// Enxadrista usa uma tabela pequena, mas isso deve dar uma boa idéia de como 
    /// funciona. Um programa de xadrez vai usar pelo menos 64 MB. Tabelas com tamanho em 
    /// gigabytes é comum.
    /// 
    /// A tabela terá várias entradas dependendo da memória disponível. Normalmente, o 
    /// número de entradas é calculado quando o programa é iniciado com base no tamanho da tabela.
    /// Cada entrada terá quatro registros, onde podemos armazenar informações sobre a posição.
    /// Cada registro terá informações sobre uma posição diferente.
    /// Esta estrutura pode variar de programa para programa.
    /// A posição é identificada pela chave "zobrist", você pode ver como é calculada na
    /// classe zobrist.cs.
    /// Vamos armazenar no registro, a profundidade, o valor da posição, o melhor movimento
    /// e um valor para indicar qual o tipo de valor que temos. Veja a classe Registro para
    /// mais detalhes sobre estes dados.
    /// 
    /// É pouco difícil implementar a tabela de transposição. Algo que pode ajudar são as 
    /// seguintes posições, onde é muito difícil para o programa encontrar o melhor movimento 
    /// sem a implementação correta. Então, use a posição e execute seu programa, 
    /// se não encontrar a melhor jogada de forma consistente com algum tempo de procura,
    /// você pode ter um problema com sua implementação. Eu sempre uso uns 20 segundos 
    /// para a procura.
    /// 
    ///     Posição                                     Melhor movimento
    ///     8/k/3p4/p2P1p2/P2P1P2/8/8/K7 w - - 0 1      a1b1
    ///     2k5/8/1pP1K3/1P6/8/8/8/8 w - -              c6c7
    /// 
    /// </remarks>
    /// <see cref="Transposicao.Registro"/>
    /// <see cref="Zobrist"/>
    public class Transposicao
    {
        /// <summary>
        /// Registro da Tabela de Transposição.
        /// </summary>
        /// <remarks>
        /// Podemos ter até quatro registros em cada entrada.
        /// Cada registro possui informações sobre uma posição.
        /// </remarks>
        public class Registro
        {
            /// <summary>
            /// Chave Zobrist exclusiva para a posição.
            /// </summary>
            public ulong Chave = 0;

            /// <summary>
            /// Profundidade onde o valor da posição foi encontrado.
            /// </summary>
            public int Profundidade = 0;

            /// <summary>
            /// Valor da posição.
            /// </summary>
            public int Valor = 0;

            /// <summary>
            /// Melhor movimento para a posição.
            /// </summary>
            public Movimento Movimento = null;

            /// <summary>
            /// Número que indica a idade do registro. Usado para substituir registros antigos.
            /// </summary>
            public byte Geracao = 0;

            /// <summary>
            /// Tipo do registro: superior, inferior ou exato.
            /// </summary>
            public byte Tipo = 0;

            /// <summary>
            /// Indica se o valor no registro pode ser usado na pesquisa.
            /// </summary>
            /// <remarks>
            /// Mesmo quando você encontra o registro na tabela de transposição, você ainda 
            /// precisa verificar se você pode usar o valor da tabela.
            /// 
            /// Quando o valor é armazenado na tabela, também armazenamos o tipo do valor:
            /// - Tipo Exato: Indica que o valor foi resultado de uma busca completa de 
            ///   todos os movimentos na posição, ou seja, o valor estava entre alfa e beta 
            ///   daquela pesquisa.
            /// - Tipo Superior: indica que o valor é o máximo para a posição, e foi inferior 
            ///   ao valor alfa no momento em que foi armazenado.
            /// - Tipo Inferior: indica que o valor estava acima de beta, o que significa que  
            ///   não tenha pesquisado todos os movimentos, porque este valor causou o corte beta.
            ///   
            /// Veja Pesquisa.cs, na função AlfaBeta para ver quando os valores são armazenados.
            /// 
            /// Portanto, para usar o valor da tabela de transposição, precisamos analisar o 
            /// valor e o tipo, de acordo com valores atuais de alfa e beta.
            /// 
            /// - Se o valor da tabela for menor ou superior ao alfa atual e o tipo for superior
            ///   ou exato, significa que podemos usá-lo, porque se pesquisarmos provavelmente 
            ///   não seremos capazes de obter um valor acima do alfa atual.
            /// 
            /// - Se o valor da tabela for maior ou igual a beta atual, e o tipo for inferior 
            ///   ou exato, podemos usar porque já temos um valor acima do beta atual. 
            ///   Se pesquisarmos, não obteremos um valor menor. Esta é uma boa situação aqui,
            ///   porque vamos causar um corte beta.
            /// 
            /// Mais uma vez, é muito importante fazer a implementação correta da tabela de 
            /// transposição, Desculpe repetir o mesmo aviso, mas todos os componentes devem 
            /// estar corretos para obter os bons movimentos. Você precisa ser persistente e 
            /// paciente para construir um bom motor de xadrez.
            /// 
            /// </remarks>
            /// 
            /// <see cref="Pesquisa.AlfaBeta(int, int, int, int, List{Movimento})"/>
            /// 
            /// <param name="alfa">Valor atual de alfa da pesquisa.</param>
            /// <param name="beta">Valor atual de beta da pesquisa.</param>
            /// <returns>Verdadeiro se o valor na table pode ser usado pela pesquisa.</returns>
            public bool PodeUsarValor(int alfa, int beta)
            {
                if (Tipo == Transposicao.Tipo.SUPERIOR && Valor <= alfa) return true;
                if (Tipo == Transposicao.Tipo.INFERIOR && Valor >= beta) return true;
                if (Tipo == Transposicao.Tipo.EXATO && Valor <= alfa) return true;
                if (Tipo == Transposicao.Tipo.EXATO && Valor >= beta) return true;
                return false;
            }
        }

        /// <summary>
        /// Número de entradas para a tabela.
        /// </summary>
        /// <remarks>
        /// Normalmente, o tamanho da tabela é alocado dinamicamente, tendo um parâmetro 
        /// para indicar a quantidade de memória disponível, como 64 MB e acima.
        /// Enxadrista está usando uma implementação simples.
        /// </remarks>
        public const int NUMERO_ENTRADAS = 500000;
        /// <summary>
        /// Número de registros em cada entrada.
        /// </summary>
        /// <remarks>
        /// É comum ter mais de um registro em cada entrada de tabela.
        /// </remarks>
        public const int NUMERO_REGISTROS = 4;

        /// <summary>
        /// Tipo de valor na tabela de transposição.
        /// </summary>
        public class Tipo
        {
            public const byte SUPERIOR = 1;
            public const byte INFERIOR = 2;
            public const byte EXATO = 3;
        }

        /// <summary>
        /// Tabela de transposição.
        /// </summary>
        public Registro[][] Tabela = new Registro[NUMERO_ENTRADAS][];

        /// <summary>
        /// Valor que indica o valor da geração de registro.
        /// </summary>
        /// <remarks>
        /// Esse valor será usado para indicar os registros mais antigos, 
        /// para que eles possam ser substituídos por registros mais recentes.
        /// Basicamente em cada pesquisa, aumentamos o valor, tornando todas as
        /// entradas mais antigas.
        /// </remarks>
        private byte Geracao;

        /// <summary>
        /// Cria a tabela de transposição.
        /// </summary>
        public Transposicao()
        {
            for (int indice_entrada = 0; indice_entrada < NUMERO_ENTRADAS; indice_entrada++) {
                Tabela[indice_entrada] = new Registro[NUMERO_REGISTROS];
                for (int indice_registro = 0; indice_registro < NUMERO_REGISTROS; indice_registro++) {
                    Tabela[indice_entrada][indice_registro] = new Registro();
                }
            }
            Geracao = 0;
        }

        /// <summary>
        /// Incrementa o contador de geração.
        /// </summary>
        public void IncrementaGeracao()
        {
            Geracao++;
        }

        /// <summary>
        /// Localiza o registro na tabela de transposição para a posição atual 
        /// usando a chave zobrist e a profundidade.
        /// </summary>
        /// <param name="chave">Chave Zobrist da posição atual.</param>
        /// <param name="profundidade">Profundidade da pesquisa, ou seja, profundidade minima que procuramos na tabela.</param>
        /// <returns>Registro da tabela, ou valor null quando não disponível.</returns>
        public Registro Recupera(ulong chave, int profundidade)
        {
            Debug.Assert(profundidade >= 0 && profundidade <= Defs.PROFUNDIDADE_MAXIMA);
            // Calcula o indice da entrada da tabela usando a função de módulo.
            int indice_entrada = (int)(chave % NUMERO_ENTRADAS);

            Debug.Assert(indice_entrada >= 0 && indice_entrada < Transposicao.NUMERO_ENTRADAS);

            var entrada = Tabela[indice_entrada];

            // Procura na entrada o primeiro registro para a chave e profundidade maior que a requerida.
            var registro = entrada.Where(r => r.Chave == chave && r.Profundidade >= profundidade).FirstOrDefault();

            // Se encountrou atualiza a geração porque é um registro útil para a pesquisa atual.
            if (registro != null) registro.Geracao = Geracao;

            return registro;
        }

        /// <summary>
        /// Salva nova informação na tabela de transposição.
        /// </summary>
        /// <param name="chave">Chave Zobrist da posição atual.</param>
        /// <param name="profundidade">Profundidade da pesquisa.</param>
        /// <param name="valor">Valor a ser salvo.</param>
        /// <param name="nivel">Nivel atual para ajustar os valores de mate.</param>
        /// <param name="tipo">Tipo do valor as ser salvo.</param>
        /// <param name="melhor">Melhor movimento para a posição ou null.</param>
        public void Salva(ulong chave, int profundidade, int valor, int nivel, byte tipo, Movimento melhor)
        {
            Debug.Assert(profundidade >= 0 && profundidade <= Defs.PROFUNDIDADE_MAXIMA);
            Debug.Assert(valor >= Defs.VALOR_MINIMO && valor <= Defs.VALOR_MAXIMO);
            Debug.Assert(nivel >= 0 && nivel < Defs.NIVEL_MAXIMO);
            Debug.Assert(tipo == Transposicao.Tipo.INFERIOR || tipo == Transposicao.Tipo.EXATO || tipo == Transposicao.Tipo.SUPERIOR);

            // Define o índice para a localização da entrada. 
            int indice_entrada = (int)(chave % NUMERO_ENTRADAS);
            Debug.Assert(indice_entrada >= 0 && indice_entrada < Transposicao.NUMERO_ENTRADAS);

            var entrada = Tabela[indice_entrada];

            // Tenta achar um registro existente para esta posição.
            var registro = entrada.FirstOrDefault(r => r.Chave == chave);

            // Evita perder os melhores movimentos que estão na tabela.
            if (registro != null && melhor == null) melhor = registro.Movimento;

            // Se é uma posição nova, então tenta substituir o registro mais antigo, com a menor profundidade.
            if (registro == null) registro = entrada.OrderBy(r => r.Geracao).ThenBy(r => r.Profundidade).First();

            // Salva os dados. 
            registro.Chave = chave;
            registro.Geracao = Geracao;
            registro.Profundidade = profundidade;
            registro.Tipo = tipo;
            registro.Movimento = melhor;
            registro.Valor = Transposicao.AjustaValorParaTabela(valor, nivel);
        }

        /// <summary>
        /// Ajusta os valores de mate para salvar na tabela.
        /// </summary>
        /// <remarks>
        /// Remove o nível atual, porque precisamos ajustar quando for usar em outros níveis. 
        /// </remarks>
        /// <see cref="Pesquisa.AlfaBeta(int, int, int, int, List{Movimento})"/>
        /// <param name="valor">Valor da pesquisa.</param>
        /// <param name="nivel">Níveis a ser removido em caso de valores de mate.</param>
        /// <returns>Valor ajustado.</returns>
        public static int AjustaValorParaTabela(int valor, int nivel)
        {
            if (valor > Defs.AVALIACAO_MAXIMA) return valor + nivel;
            if (valor < Defs.AVALIACAO_MINIMA) return valor - nivel;
            return valor;
        }

        /// <summary>
        /// Ajusta o valor de mate para incluir o nível atual.
        /// </summary>
        /// <see cref="Pesquisa.AlfaBeta(int, int, int, int, List{Movimento})"/>
        /// <param name="valor">Valor da tabela a ser adjustado.</param>
        /// <param name="nivel">Nível atual a ser incluido no valor.</param>
        /// <returns>Valor adjustado.</returns>
        public static int AjustaValorParaProcura(int valor, int nivel)
        {
            if (valor > Defs.AVALIACAO_MAXIMA) return valor - nivel;
            if (valor < Defs.AVALIACAO_MINIMA) return valor + nivel;
            return valor;
        }

    }
}

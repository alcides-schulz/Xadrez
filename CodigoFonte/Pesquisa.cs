using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Enxadrista
{
    /// <summary>
    /// Funcionalidade para encontrar o melhor movimento.
    /// </summary>
    /// <remarks>
    /// Esta é a parte que coordena os componentes do motor de xadrez para encontrar o melhor movimento.
    /// Usa o tabuleiro para fazer e desfazer movimentos, usa a avaliação para dar valor às posições e 
    /// seleciona o melhor movimento resultante desse processo.
    /// Basicamente, é assim que funciona, digamos que começamos com o branco para jogar, o algoritmo mantém 
    /// dois valores, alfa e beta, que representam a pontuação mínima que o jogador maximizador (branco) é 
    /// assegurado e a pontuação máxima que o jogador minimizador ( preto) é assegurada, respectivamente. 
    /// Inicialmente, o alfa é o infinito negativo e o beta é o infinito positivo, ou seja, ambos os jogadores 
    /// começam com o seu pior resultado possível. Sempre que a pontuação máxima que o jogador minimizador (balck)
    /// garanta seja menor do que a pontuação mínima que o jogador maximizador (branco) é assegurado, 
    /// ou seja, beta menor ou igual a alfa, o jogador maximizador (branco) não precisa considerar os 
    /// descendentes de este nó, pois nunca serão alcançados na jogada real. Este é o corte beta e é um dos 
    /// principais objetivos da pesquisa, porque evitamos procura posições menos importantes.
    /// Este processo irá se repetir recursivamente invertendo os jogadores e os valores alfa e beta.
    /// É um pouco complicado, requer algum tempo para estudar e entender, mas depois de algum tempo você deve 
    /// pegar o jeito. Eu recomendo olhar para a implementação dos motores de xadrez escritos mais recentes, 
    /// a implementação é mais limpa do que os motores antigos. Eu tentei manter a pesquisa do Enxadrista 
    /// o mais simples possível.
    /// A pesquisa inicia no método LoopAprofundamentoIterativo.
    /// </remarks>
    /// <see cref="LoopAprofundamentoIterativo(int, int)"/>
    public class Pesquisa
    {
        // Componentes do motor necessários para a pesquisa.
        public Tabuleiro Tabuleiro;
        public Avaliacao Avaliacao;
        public Transposicao Transposicao;
        public Ordenacao Ordenacao = new Ordenacao();

        // Dados usados para controlar a pesquisa.
        public bool ImprimeInformacao = true;
        public ulong ContadorPosicoes = 0;
        public bool EncerraProcura = false;
        public int MilisegundosLimite = 0;
        public int ProfundidadeLimite = 0;
        public int ProfundidadeAtual = 0;
        public Movimento MelhorMovimento = null;
        private Stopwatch ControleTempo = new Stopwatch();

        /// <summary>
        /// Cria componente the pesquisa e associa aos outros componentes do motor.
        /// </summary>
        /// <param name="tabuleiro">Tablueiro.</param>
        /// <param name="avaliacao">Avaliação.</param>
        /// <param name="transposicao">Tabela de Transposição.</param>
        public Pesquisa(Tabuleiro tabuleiro, Avaliacao avaliacao, Transposicao transposicao)
        {
            Transposicao = transposicao;
            Tabuleiro = tabuleiro;
            Avaliacao = avaliacao;
        }

        /// <summary>
        /// Inicia uma nova pesquisa para a posição atual no tabuleiro, e encontra o melhor 
        /// movimento para o jogador na vez.
        /// O movimento encontrado será salvo em MelhorMovimento.
        /// </summary>
        /// <remarks>
        /// Este processe repete a pesquisa incrementando a profundidade a cada iteração.
        /// Sim, é exatamente como você leu. Vamos procurar com depth = 1, depois repetir com depth = 2,
        /// repetir com depth = 3 e assim por diante. Pode parecer ineficiente, mas cada iteração usará
        /// informações coletadas na iteração anterior e acelera a proxima iteração.
        /// Esta repetição será limitada por tempo ou profundidade. Então, verificaremos o fim da pesquisa 
        /// durante o processo.
        /// Algo que pode melhorar aqui, é usar um controle de tempo melhor, onde você aloca mais ou menos
        /// tempo com base no estágio do jogo, ou estende se o movimento retornou uma pontuação ruim, etc.
        /// O gerenciamento de tempo é um capítulo a parte para motores de xadrez. É como um jogador de xadrez
        /// vai alocar mais tempo para avaliar movimentos importantes.
        /// Note que aqui recebemos o tempo limite já calculado, o gerenciamento de tempo deve ser feito 
        /// antes de entrar na pesquisa, e pode ser ajustado de acordo com desenrolar da pesquisa.
        /// Outra técnica é a janela de aspiração, onde você pode começar com pequenas janelas e ampliar,
        /// conforme necessário, isso deve economizar tempo porque você não pesquisa as posições com valores
        /// fora da janela. A janela de pesquisa é definida pelo intervalo entre os valores de alfa e beta.
        /// </remarks>
        /// <param name="milisegundo_limite">Tempo limite para a pesquisa em milisegundos.</param>
        /// <param name="profundidade_limite">Profundidade limite para a pesquisa.</param>
        public void LoopAprofundamentoIterativo(int milisegundo_limite, int profundidade_limite)
        {
            Debug.Assert(milisegundo_limite >= 0);
            Debug.Assert(profundidade_limite > 0 && profundidade_limite <= Defs.PROFUNDIDADE_MAXIMA);

            ControleTempo.Restart();

            Transposicao.IncrementaGeracao();
            Ordenacao.Inicia();

            ContadorPosicoes = 0;
            MilisegundosLimite = milisegundo_limite;
            ProfundidadeLimite = profundidade_limite;
            EncerraProcura = false;
            MelhorMovimento = null;

            var variacao_principal = new List<Movimento>();

            for (ProfundidadeAtual = 1; ProfundidadeAtual <= Defs.PROFUNDIDADE_MAXIMA; ProfundidadeAtual++) {
                AlfaBeta(Defs.VALOR_MINIMO, Defs.VALOR_MAXIMO, 0, ProfundidadeAtual, variacao_principal);
                if (EncerraProcura) break;
                // Não inicia um novo loop se já usamos 60% do tempo disponível. É provável que não possamos terminar.
                if (ControleTempo.ElapsedMilliseconds > (int)(MilisegundosLimite * 0.60)) break;
                // Libera memoria aos poucos. Provavelmente esta parte pode ser melhorada.
                GC.Collect();
            }

            ControleTempo.Stop();

            MelhorMovimento = variacao_principal.Count > 0 ? variacao_principal[0] : null;
        }

        /// <summary>
        /// Executa a pesquisa alfa beta.
        /// </summary>
        /// <remarks>
        /// Essa função deve ser a mais executada para o motor de xadrez. A maior parte da diversão 
        /// acontece aqui, o que significa que há muitas técnicas que podem ser tentadas.
        /// Vamos comentar as técnicas diretamente no código abaixo, mas a idéia geral é passar mais
        /// tempo procurando movimentos promissores e passar menos tempo buscando possíveis movimentos
        /// ruins.
        /// A busca às vezes descarta alguns movimentos, e às vezes estende outros. Por exemplo, estender
        /// a busca por um movimento de xeque é geralmente bom. Evitar pesquisar movimentos no final da 
        /// lista de movimentos também é geralmente bom.
        /// Há sempre algumas idéias que podem ser tentadas, quando você no código de outros motores você
        /// pode obter alguma inspiração para suas próprias idéias e tentar no seu programa. A maioria das
        /// ideias em outros motores tem que ser adaptada ao seu programa, às vezes eles simplesmente não
        /// funcionam, porque seu programa possui uma estrutura diferente. 
        /// E um ponto importante, que é considerado cortesia, quando você menciona de onde você teve a
        /// idéia / inspiração. 
        /// </remarks>
        /// <param name="alfa">Limite inferior da pesquisa.</param>
        /// <param name="beta">Limite superior da pesquisa.</param>
        /// <param name="nivel">Distância da posição inicial (conhecido como ply). Aumentada a cada chamada.</param>
        /// <param name="profundidade">Profundidade da pesquisa (depth), número de movimentos para olhar a frente. Diminui a cada chamada.</param>
        /// <param name="variacao_principal">Lista dos melhores movimentos localizados durante a pesquisa.</param>
        /// <returns>Melhor valor encontrado para a posição.</returns>
        public int AlfaBeta(int alfa, int beta, int nivel, int profundidade, List<Movimento> variacao_principal)
        {
            Debug.Assert(alfa >= Defs.VALOR_MINIMO);
            Debug.Assert(beta <= Defs.VALOR_MAXIMO);
            Debug.Assert(beta > alfa);
            Debug.Assert(nivel >= 0 && nivel <= Defs.NIVEL_MAXIMO);
            Debug.Assert(profundidade >= 0 && profundidade <= Defs.PROFUNDIDADE_MAXIMA);
            Debug.Assert(variacao_principal != null);

            // Final da pesquisa por causa do tempo ou profundidade ?
            VerificaTerminoProcura();
            if (EncerraProcura) return 0;

            // Chegamos a uma posição que é empate por causa das regras do xadrez. 
            if (nivel > 0 && Tabuleiro.EmpatePorRegra50()) return 0;
            if (nivel > 0 && Tabuleiro.EmpatePorRepeticao()) return 0;

            // Ao chegar ao final da pesquisa, vai para a pesquisa quiescente para obter o resultado final.
            if (profundidade <= 0) return Quiescente(alfa, beta, nivel, variacao_principal);

            // Contador de posição. Apenas para fins informativos, para que você veja o quão rápido é o seu motor. 
            ContadorPosicoes++;

            // Prepara lista the movimentos encontrados.
            if (nivel > 0) variacao_principal.Clear();

            // Programação defensiva para evitar erros de índice fora da faixa da tabela.
            if (nivel > Defs.NIVEL_MAXIMO - 1) return Avaliacao.ObtemPontuacao();

            // Accesso a informações da tabela de transposição (Transposition Table Probe).
            // Nota: alguns programas usam Hash Table ao inves the Transposition Table.
            // Se esta posição foi visitada antes, a pontuação pode estar na tabela de transposição,
            // podemos reutilizá-la e retornar daqui. Esta é uma grande economia.
            // Esta informação será salva durante a pesquisa, quando tivermos a informação sobre a pontuação.
            // É importante usar o valor somente se a profundidade na tabela for maior ou igual à profundidade atual.
            // Se não podemos usar a pontuação, podemos usar o melhor movimento, que pode causar um corte beta e 
            // também economizar tempo. Lembre-se de que isso ajuda na ordenação do movimentos.
            Movimento movimento_transposicao = null;
            var registro = Transposicao.Recupera(Tabuleiro.Chave, profundidade);
            if (registro != null) {
                if (registro.PodeUsarValor(alfa, beta)) {
                    return Transposicao.AjustaValorParaProcura(registro.Valor, nivel);
                }
                movimento_transposicao = registro.Movimento;
            }

            // Preparação de informações para a pesquisa.
            var cor_jogar_esta_em_cheque = Tabuleiro.CorJogarEstaEmCheque();
            var valor_avaliacao = Avaliacao.ObtemPontuacao();
            var nova_variacao_principal = new List<Movimento>();

            // "Passar a navalha" (Razoring).
            // Talvez a tradução de razoring não seja boa, mas aqui vamos tentar remover posições 
            // que não são muito promissoras, com uma busca reduzida. Vamos tentar passar a navalha nessas posições !
            // O valor de avaliação mais um valor estimado já é menor do que o alfa, então, se a busca reduzida
            // confirmar que não há uma boa captura, podemos descartar essa posição e ignorar a pesquisa.
            if (profundidade <= 3 && !cor_jogar_esta_em_cheque && valor_avaliacao + 150 * profundidade < alfa) {
                int alfa_reduzido = alfa - 150 * profundidade;
                int valor = Quiescente(alfa_reduzido, alfa_reduzido + 1, nivel, nova_variacao_principal);
                if (EncerraProcura) return 0;
                if (valor <= alfa_reduzido) return valor;
            }

            // Este é o famoso movimento nulo (movimento nulo). Praticamente todos os motores de xadrez o implementam.
            // A idéia é que você está em uma posição que é tão boa (valor maior do que beta) que mesmo se você passar
            // o direito de mover para o outro jogador, dando-lhe dois movimentos em sequência, ele não será capaz de recuperar.
            // Então, você pode descartar essa pesquisa. É um bom ganho.
            // Existem algumas boas práticas para movimentos nulos:
            // - Não faça dois movimentos nulos em seqüência.
            // - Não pode estar no xeque, pois pode gerar posições ilegais.
            // - É bom que você tenha algumas peças.
            // - Também não é recomendado para pesquisa de variação principal.
            // O movimento nulo deve ser confirmado com uma busca reduzida.
            // O valor obtido pode ser salvo na tabela de transposição.
            if (profundidade > 3 && !cor_jogar_esta_em_cheque && alfa == beta - 1 && valor_avaliacao >= beta) { 
                if (!Tabuleiro.MovimentoAnteriorFoiNulo() && Tabuleiro.TemPecas(Tabuleiro.CorJogar)) {
                    Tabuleiro.FazMovimentoNulo();
                    int valor = -AlfaBeta(-beta, -beta + 1, nivel + 1, profundidade - 3, nova_variacao_principal);
                    Tabuleiro.DesfazMovimentoNulo();
                    if (EncerraProcura) return 0;
                    if (valor >= beta) {
                        if (valor > Defs.AVALIACAO_MAXIMA) valor = beta; // // Valor de mate, não muito confiável neste caso.
                        Transposicao.Salva(Tabuleiro.Chave, profundidade, valor, nivel, Transposicao.REGISTRO_INFERIOR, null);
                        return valor;
                    }
                }
            }

            // Prepara a próxima profundidade. Se estiver em xeque, estendemos a pesquisa.
            // Esta é uma extensão de xeque simples, pode ser implementada em diferentes formas.
            int nova_profundidade = profundidade - 1;
            if (cor_jogar_esta_em_cheque) nova_profundidade += 1;

            // Mais itens de controle de pesquisa.
            int melhor_valor = Defs.VALOR_MINIMO;
            int contador_movimentos = 0;
            Movimento melhor_movimento = null;

            // Gera e classifica a lista de movimentos. Usa o movimento da tabela de transposição, se disponível.
            var lista = Tabuleiro.GeraMovimentos();
            lista = Ordenacao.Orderna(Tabuleiro.CorJogar, lista, movimento_transposicao);

            // Loop dos movimentos.
            foreach (var movimento in lista) {
                Tabuleiro.FazMovimento(movimento);
                if (!Tabuleiro.MovimentoFeitoLegal()) {
                    Tabuleiro.DesfazMovimento();
                    continue;
                }
                Debug.Assert(Tabuleiro.Chave == Zobrist.ObtemChave(Tabuleiro));

                contador_movimentos += 1;

                // Início da pesquisa recursiva. O objectivo é obter o valor para este movimento nesta profundidade.
                int valor_procura = 0;
                if (melhor_valor == Defs.VALOR_MINIMO) {
                    // O primeiro movimento será pesquisado com o tamanho da janela inteira, ou seja, os valores alfa / beta invertidos.
                    valor_procura = -AlfaBeta(-beta, -alfa, nivel + 1, nova_profundidade, nova_variacao_principal);
                    // Os movimentos seguintes são pesquisados com uma pesquisa de janela zero. Em vez de usar -beta, -alfa para a 
                    // janela de pesquisa, usaremos -alfa-1, -alfa. Isso deve descartar mais movimentos porque os valores 
                    // alfa / beta da próxima iteração serão próximos. Mas antes podemos aplicar algumas técnicas para reduzir a 
                    // quantidade de movimentos pesquisados. Veja abaixo na parte do "else".
                }
                else {
                    // Poda de futilidade - Futility Pruning.
                    // Se o valor da avaliação mais um valor estimado for inferior ao valor alfa, podemos ignorar esse movimento.
                    // Mas temos que considerar algumas restrições como abaixo. Existem muitas maneiras diferentes de implementar 
                    // esta técnica, mais uma vez você pode ajustar de acordo com sua preferência e se ela funcionar para o seu motor. 
                    if (!cor_jogar_esta_em_cheque && nova_profundidade == 1 && !movimento.Tatico() && alfa == beta - 1 && valor_avaliacao + 100 < alfa) {
                        Tabuleiro.DesfazMovimento();
                        continue;
                    }

                    // Redução de movimento tardios - Late move reduction ou LMR.
                    // Para movimentos que estão mais próximos do fim da lista, podemos reduzir a profundidade em que são pesquisados. 
                    // Esta técnica também pode ser adaptada de muitas formas diferentes.
                    int reducao = 0;
                    if (!cor_jogar_esta_em_cheque && nova_profundidade > 1 && contador_movimentos > 4 && !movimento.Tatico() && alfa == beta - 1 && valor_avaliacao < alfa) {
                        reducao = 1;
                    }

                    // Outras técnicas de conhecimento podem ser aplicadas aqui. E talvez você possa criar uma nova técnica!

                    // Executa a pesquisa de janela zero.
                    valor_procura = -AlfaBeta(-alfa - 1, -alfa, nivel + 1, nova_profundidade - reducao, nova_variacao_principal);

                    // Quando a busca foi reduzida e o valor retornado é maior do que o alfa, significa que podemos ter um bom
                    // movimento, e temos que pesquisar sem a redução para confirmar. Esperava-se que não tivesse um bom movimento
                    // neste caso.
                    if (!EncerraProcura && valor_procura > alfa && reducao != 0) {
                        valor_procura = -AlfaBeta(-alfa - 1, -alfa, nivel + 1, nova_profundidade, nova_variacao_principal);
                    }

                    // Este é outro caso de re-pesquisa, depois de pesquisar com janela zero. Esperava-se não encontrar bons movimentos 
                    // neste caso. Se o valor retornado for maior que o alfa, significa que devemos pesquisar novamente com a janela 
                    // completa, para confirmar o movimento bom.
                    if (!EncerraProcura && valor_procura > alfa && valor_procura < beta) {
                        valor_procura = -AlfaBeta(-beta, -alfa, nivel + 1, nova_profundidade, nova_variacao_principal);
                    }
                    // Nota: Pode parecer que o motor pode pesquisar a mesma posição duas vezes, mas se você seguir a lógica acima, 
                    // será uma ou outra.
                }

                Tabuleiro.DesfazMovimento();
                if (EncerraProcura) return 0;
                Debug.Assert(Tabuleiro.Chave == Zobrist.ObtemChave(Tabuleiro));

                // Agora que temos o valor para este movimento nesta posição, podemos comparar a alfa e beta tomar decisões.
                // Se o valor for maior que o beta, temos um corte beta. Isso significa que não precisamos pesquisar o resto dos
                // movimentos. O objetivo é obter muitos cortes beta, especialmente no primeiro movimento.
                // É importante atualizar o histórico de movimentos e a tabela de transposição, então quando pesquisamos essa
                // posição novamente, possivelmente também podemos ter outro corte beta.
                if (valor_procura >= beta) {
                    Ordenacao.AtualizaHistoria(Tabuleiro.CorJogar, movimento, profundidade);
                    Transposicao.Salva(Tabuleiro.Chave, profundidade, valor_procura, nivel, Transposicao.REGISTRO_INFERIOR, movimento);
                    return valor_procura;
                }

                // Se não tivermos um corte beta, temos que gravar o melhor resultado até agora, e aumentar o valor da alfa. Isso significa 
                // que temos uma boa jogada para o cargo, e podemos atualizar a variação principal.
                if (valor_procura > melhor_valor) {
                    melhor_valor = valor_procura;
                    if (valor_procura > alfa) {
                        alfa = valor_procura;
                        melhor_movimento = movimento;
                        AtualizaVariacaoPrincipal(variacao_principal, nova_variacao_principal, movimento);
                        if (nivel == 0) ImprimeVariacaoPrincipal(valor_procura, profundidade, variacao_principal);
                    }
                }
            }

            // Acabamos de pesquisar todos movimentos. Precisamos fazer mais alguns passos.

            // Se todos os movimentos forem ilegais, significa que não podemos mover nessa posição.
            // Se o nosso rei estiver em xeque, então é xeque mate, esta posição está perdida, e nós retornamos uma pontuação de xeque.
            // Se o nosso rei não estiver em xeque, então é empate. Não podemos fazer um movimento sem colocar o rei no xeque.
            if (melhor_valor == Defs.VALOR_MINIMO)
                return cor_jogar_esta_em_cheque ? -Defs.VALOR_MATE + nivel : 0;

            // Aqui vamos atualizar o histórico de movimentos e a tabela de transposição de acordo com o escore.
            // Podemos encontrar uma bom movimento, ou nenhum movimento foi capaz de melhorar alfa.
            if (melhor_movimento != null) {
                Ordenacao.AtualizaHistoria(Tabuleiro.CorJogar, melhor_movimento, profundidade);
                Transposicao.Salva(Tabuleiro.Chave, profundidade, melhor_valor, nivel, Transposicao.REGISTRO_EXATO, melhor_movimento);
            }
            else {
                Transposicao.Salva(Tabuleiro.Chave, profundidade, melhor_valor, nivel, Transposicao.REGISTRO_SUPERIOR, null);
            }

            // Retorna o valor da posição.
            return melhor_valor;
        }

        public int Quiescente(int alfa, int beta, int nivel, List<Movimento> variacao_principal)
        {
            Debug.Assert(alfa >= Defs.VALOR_MINIMO);
            Debug.Assert(beta <= Defs.VALOR_MAXIMO);
            Debug.Assert(beta > alfa);
            Debug.Assert(nivel >= 0 && nivel <= Defs.NIVEL_MAXIMO);
            Debug.Assert(variacao_principal != null);

            VerificaTerminoProcura();
            if (EncerraProcura) return 0;
            ContadorPosicoes++;
            if (nivel > 0) variacao_principal.Clear();

            if (nivel >= Defs.NIVEL_MAXIMO - 1) return Avaliacao.ObtemPontuacao();

            int melhor_valor = Avaliacao.ObtemPontuacao();
            if (melhor_valor >= beta) return melhor_valor;
            if (melhor_valor > alfa) alfa = melhor_valor;

            var nova_variacao_principal = new List<Movimento>();

            var lista = Tabuleiro.GeraMovimentos();
            lista = Ordenacao.Orderna(Tabuleiro.CorJogar, lista, null);

            foreach (var movimento in lista) {
                if (!movimento.Captura() && !movimento.Promocao()) continue;

                Tabuleiro.FazMovimento(movimento);
                if (!Tabuleiro.MovimentoFeitoLegal()) {
                    Tabuleiro.DesfazMovimento();
                    continue;
                }
                int valor_procura = -Quiescente(-beta, -alfa, nivel + 1, nova_variacao_principal);
                Tabuleiro.DesfazMovimento();
                if (EncerraProcura) return 0;

                if (valor_procura >= beta) return valor_procura;

                if (valor_procura > melhor_valor) {
                    melhor_valor = valor_procura;
                    if (valor_procura > alfa) {
                        alfa = valor_procura;
                        AtualizaVariacaoPrincipal(variacao_principal, nova_variacao_principal, movimento);
                    }
                }
            }

            return melhor_valor;
        }

        private void VerificaTerminoProcura()
        {
            if (ContadorPosicoes % 2000 != 0) return;
            if (ControleTempo.ElapsedMilliseconds >= MilisegundosLimite) EncerraProcura = true;
            if (ProfundidadeAtual >= ProfundidadeLimite) EncerraProcura = true;
        }

        private void AtualizaVariacaoPrincipal(List<Movimento> destino, List<Movimento> origem, Movimento movimento)
        {
            Debug.Assert(destino != null);
            Debug.Assert(origem != null);
            Debug.Assert(movimento != null);

            destino.Clear();
            destino.Add(movimento);
            foreach (var novo_movimento in origem) destino.Add(novo_movimento);
        }

        private void ImprimeVariacaoPrincipal(int valor, int profundidade, List<Movimento> variacao_principal)
        {
            Debug.Assert(valor >= Defs.VALOR_MINIMO && valor <= Defs.VALOR_MAXIMO);
            Debug.Assert(profundidade > 0 && profundidade <= Defs.PROFUNDIDADE_MAXIMA);
            Debug.Assert(variacao_principal != null);

            if (!ImprimeInformacao) return;
            Console.Write(profundidade);
            Console.Write(" " + valor);
            Console.Write(" " + String.Format("{0:0.0000}", (Decimal)(ControleTempo.ElapsedMilliseconds / 1000.0)));
            Console.Write(" " + ContadorPosicoes);
            foreach (var movimento in variacao_principal) Console.Write(" " + movimento.Notacao());
            Console.WriteLine();
            Console.Out.Flush();
        }

    }
}

namespace Enxadrista
{
    /// <summary>
    /// Avalia a posição atual do tabuleiro e atribui um valor. 
    /// </summary>
    /// <remarks>
    /// Esta avaliação será usada pela pesquisa para selecionar a melhor jogada. O valor resultante de uma pesquisa é a
    /// avaliação de uma posição folha na arvore, mas também podemos considerar a avaliação como um guia já que a busca
    /// deve concentrar-se em tabuleiros com as melhores pontuações (para o lado que está jogando).
    ///  
    /// Valores positivos são dados a termos que são considerados bônus e valores negativos para penalidades. Já a
    /// importância é definida pelo valor atribuído, quanto mais próximo a zero menor a importância do termo.
    /// Somando os bônus e penalidades para cada lado chega-se ao valor resultante. 
    /// 
    /// Para cada termo avaliado existe um custo computacional na identificação do comportamento no tabuleiro, por isso
    /// é melhor usar termos que acontecem com mais frequência ou com bastante importância, já que usar um termo muito
    /// específico e com pouca importância pode afetar negativamente o desempenho.
    /// 
    /// Os termos mais comuns em uma avaliação são:
    /// - material - Cada peça tem seu valor, com isso podemos definir que uma Rainha vale mais que um Cavalo.
    /// - segurança do rei - Bonus para peças atacando casas próximas ao Rei, possíveis cheques, etc.
    /// - estrutura do peão - Penalidades para peões isolados, peões duplicados, etc. Bônus para peões conectados, 
    ///   passados, etc.
    /// - mobilidade da peça - Quanto mais movimentos disponíveis para uma peça, melhor.
    /// 
    /// Há muitos outros termos, tente colocar no seu motor termos que melhoram sua performance. É muito importante
    /// automatizar a atribuição de valores dos termos. É muito difícil selecionar manualmente valores corretos para
    /// cada termo e já existem alguns processos de ajuste automático. O "Método de afinação Texel"
    /// ("Texel tuning method") é bem simples e popular. Vários motores já utilizaram este método com sucesso, como
    /// por exemplo, o Tucano e o Pirarucu.
    /// 
    /// A avaliação que vamos mostrar é muito simples e pode ser melhorada pela adição de:
    /// - mobilidade das peças: Contando quantas casas as peças podem alcançar. Você também pode 
    ///   adicionar o número de peças próprias que defende ou o número de peças inimigas que ataca. Isso influencia o
    ///   estilo de jogo, pode tentar atacar mais ou defender mais. Isso é algo a ser testado.
    /// - Estrutura de peão: penalize peões isolados, peões duplos e dê bônus para peões passados. 
    /// 
    /// Para cada mudança feita na avaliação é necessário testar se o impacto da mudança é positivo, afinal só queremos
    /// usar termos que dão uma melhoria na performance. Não gaste muito tempo procurando valores perfeitos para cada
    /// termo, use valores que melhoram a performance. 
    ///  
    /// </remarks>
    public class Avaliacao
    {
        /// <summary>
        /// Tabuleiro a ser avaliado.
        /// </summary>
        public Tabuleiro Tabuleiro;

        /// <summary>
        /// Mantém a pontuação do lado branco.
        /// </summary>
        public Pontuacao Branco = new Pontuacao();

        /// <summary>
        /// Mantém a pontuação do lado preto.
        /// </summary>
        public Pontuacao Preto = new Pontuacao();

        /// <summary>
        /// Número para calcular a fase do jogo, com base no material disponível.
        /// </summary>
        /// <remarks>
        /// A idéia é projetar a pontuação para o valor do material no tabuleiro. Alguns valores são 
        /// melhores no início do jogo, como a segurança do rei, e outros são melhores para o final do jogo, 
        /// como o adiantamento dos peões. O valor da fase será ajustado de acordo com o material no tabuleiro 
        /// e usado para ajustar a pontuação.
        /// </remarks>
        /// <see cref="CalculaPontuacaoFinal"/>
        public int Fase;

        /// <summary>
        /// Parâmetros de avaliação de peoes.
        /// </summary>
        /// <remarks>
        /// A avaliação dos peoes precisa especificar parâmetros dependendo do lado avaliado. 
        /// Ao criar um conjunto de parâmetros para cada lado, é fácil reutilizar o método de avaliação da torre.
        /// Existem outras maneiras de fazer essa avaliação, isso é apenas uma idéia.
        /// </remarks>
        private class DadosPeao
        {
            public static readonly DadosPeao Branco = new DadosPeao
            {
                IndiceCentralAvancado1 = (int)Defs.INDICE.D4,
                IndiceCentralAvancado2 = (int)Defs.INDICE.E4,
                IndiceCentral1 = (int)Defs.INDICE.D3,
                IndiceCentral2 = (int)Defs.INDICE.E3,
            };
            
            public static readonly DadosPeao Preto = new DadosPeao
            {
                IndiceCentralAvancado1 = (int)Defs.INDICE.D5,
                IndiceCentralAvancado2 = (int)Defs.INDICE.E5,
                IndiceCentral1 = (int)Defs.INDICE.D6,
                IndiceCentral2 = (int)Defs.INDICE.E6,
            };
            
            public int IndiceCentralAvancado1;
            public int IndiceCentralAvancado2;
            public int IndiceCentral1;
            public int IndiceCentral2;
        }

        /// <summary>
        /// Parâmetros de avaliação da torre.
        /// </summary>
        private class DadosTorre
        {
            public static readonly DadosTorre Branca = new DadosTorre
            {
                IndiceFileira1 = (int)Defs.INDICE.A1,
                IndiceFileira7 = (int)Defs.INDICE.A7,
                DirecaoEmFrente = Defs.POSICAO_NORTE,
                PeaoAmigo = Peca.PeaoBranco,
                PeaoInimigo = Peca.PeaoPreto,
            };
            
            public static readonly DadosTorre Preta = new DadosTorre
            {
                IndiceFileira1 = (int)Defs.INDICE.A8,
                IndiceFileira7 = (int)Defs.INDICE.A2,
                DirecaoEmFrente = Defs.POSICAO_SUL,
                PeaoAmigo = Peca.PeaoPreto,
                PeaoInimigo = Peca.PeaoBranco,
            };
            
            public int IndiceFileira1;
            public int IndiceFileira7;
            public int DirecaoEmFrente;
            public Peca PeaoAmigo;
            public Peca PeaoInimigo;
        }

        /// <summary>
        /// Parâmetros de avaliação do rei.
        /// </summary>
        private class DadosRei
        {
            public static DadosRei Branco = new DadosRei
            {
                DirecaoEmFrente = new int[] { Defs.POSICAO_NOROESTE, Defs.POSICAO_NORTE, Defs.POSICAO_NORDESTE },
                PeaoAmigo = Peca.PeaoBranco,
                TabelaInicio = Pontuacao.Tabela.REI_BRANCO_INICIO,
            };
            
            public static DadosRei Preto = new DadosRei
            {
                DirecaoEmFrente = new int[] { Defs.POSICAO_SUDOESTE, Defs.POSICAO_SUL, Defs.POSICAO_SUDESTE },
                PeaoAmigo = Peca.PeaoPreto,
                TabelaInicio = Pontuacao.Tabela.REI_PRETO_INICIO,
            };
            
            public int[] DirecaoEmFrente;
            public Peca PeaoAmigo;
            public int[] TabelaInicio;
        }

        /// <summary>
        /// Cria nova avaliação e inicializa parâmetros.
        /// </summary>
        /// <param name="tabuleiro">Tabuleiro a ser avaliado.</param>
        public Avaliacao(Tabuleiro tabuleiro)
        {
            Tabuleiro = tabuleiro;
        }

        /// <summary>
        /// Avalia o tabuleiro de xadrez atual e retorna o valor da avaliação.
        /// </summary>
        /// <remarks>
        /// Se a cor para mover é preta, ela mudará o sinal de valor da avaliação.
        /// </remarks>
        /// <returns>Valor de avaliação de acordo com a cor para mover.</returns>
        public int ObtemPontuacao()
        {
            PreparaAvaliacao();
            AvaliaTabuleiro();

            return Tabuleiro.CorJogar.Multiplicador() * CalculaPontuacaoFinal();
        }

        /// <summary>
        /// Zera os valores para avaliação.
        /// </summary>
        private void PreparaAvaliacao()
        {
            Fase = Pontuacao.Fase.TOTAL;
            Branco.ZeraPontuacao();
            Preto.ZeraPontuacao();
        }

        /// <summary>
        /// Avalia o tabuleiro.
        /// </summary>
        /// <remarks>
        /// Percorre o tabuleiro e avalia cada peça.
        /// </remarks>
        private void AvaliaTabuleiro()
        {
            for (var fileira = Defs.PRIMEIRA_FILEIRA; fileira < Defs.ULTIMA_FILEIRA; fileira++) {

                for (var coluna = Defs.PRIMEIRA_COLUNA; coluna < Defs.ULTIMA_COLUNA; coluna++) {

                    var indice = fileira * Defs.NUMERO_COLUNAS + coluna;
                    var peca = Tabuleiro.ObtemPeca(indice);
                    switch (peca)
                    {
                        case Peca.Nenhuma: break;
                        case Peca.PeaoBranco:
                            AvaliaPeao(Branco, indice, DadosPeao.Branco); 
                            break;
                        case Peca.CavaloBranco:
                            AvaliaCavalo(Branco, indice);
                            break;
                        case Peca.BispoBranco:
                            AvaliaBispo(Branco, indice);
                            break;
                        case Peca.TorreBranca:
                            AvaliaTorre(Branco, indice, DadosTorre.Branca);
                            break;
                        case Peca.DamaBranca:
                            AvaliaDama(Branco, indice);
                            break;
                        case Peca.ReiBranco:
                            AvaliaRei(Branco, indice, DadosRei.Branco);
                            break;
                        case Peca.PeaoPreto:
                            AvaliaPeao(Preto, indice,DadosPeao.Preto); 
                            break;
                        case Peca.CavaloPreto:
                            AvaliaCavalo(Preto, indice);
                            break;
                        case Peca.BispoPreto:
                            AvaliaBispo(Preto, indice);
                            break;
                        case Peca.TorrePreta:
                            AvaliaTorre(Preto, indice, DadosTorre.Preta);
                            break;
                        case Peca.DamaPreta:
                            AvaliaDama(Preto, indice);
                            break;
                        case Peca.ReiPreto:
                            AvaliaRei(Preto, indice, DadosRei.Preto);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Avalia peões.
        /// </summary>
        /// <remarks>
        /// Valor material, valor na casa de acordo com uma tabela e bônus de centralização.
        /// Outros termos que podems ser adicionados: 
        /// Peões passados: bônus para peões que não podem ser capturados por peões inimigos.
        /// Peões isolados: penalidade para peões sem peões amigos em colunas vizinhas.
        /// Peões conectados: bônus para peões que se apoiam.
        /// Peões duplicados: penalidade para peões na mesma coluna.
        /// Peões para trás: penalidade para peões que não podem avançar sem encontrar os peões inimigos.
        /// Peões candidatos: bônus para peões que podem se tornar passados.
        /// Outra idéia é armazenar o valor da estrutura de peões em uma tabela, e quando encontramos 
        /// a mesma estrutura de peões mais tarde, podemos usar o valor calculado dessa tabela. Isso ajuda 
        /// o desempenho, pois a estrutura do peão pode repetir muito em uma pesquisa.
        /// Neste caso, podemos usar uma chave zobrist para a estrutura do peão. Normalmente, isso é 
        /// chamado de "pawn hash table".
        /// </remarks>
        /// <param name="pontuacao">Pontuação do lado branco ou preto</param>
        /// <param name="indice">Índice da casa do peão</param>
        /// <param name="dadosPeao">Informação relativa à cor a ser avaliada.</param>
        private void AvaliaPeao(Pontuacao pontuacao, int indice, DadosPeao dadosPeao)
        {
            Fase -= Pontuacao.Fase.PEAO;
            pontuacao.Inicial += Pontuacao.Material.PEAO_INICIO;
            pontuacao.Inicial += Pontuacao.Tabela.PEAO_BRANCO[Defs.Converte12x12Para8x8(indice)];
            if (indice == dadosPeao.IndiceCentralAvancado1 || indice == dadosPeao.IndiceCentralAvancado2) pontuacao.Inicial += Pontuacao.Peao.PEAO_CENTRAL_1;
            if (indice == dadosPeao.IndiceCentral1 || indice == dadosPeao.IndiceCentral2) pontuacao.Inicial += Pontuacao.Peao.PEAO_CENTRAL_2;
            pontuacao.Final += Pontuacao.Material.PEAO_FINAL;
            pontuacao.Final += Pontuacao.Tabela.PEAO_BRANCO[Defs.Converte12x12Para8x8(indice)];
        }

        /// <summary>
        /// Avalia o cavalo.
        /// </summary>
        /// <remarks>
        /// Basicamente vai tentar centralizar o cavalo usando uma tabela de valores.
        /// Normalmente o bônus para mobilidade é bom. No caso do cavalo evite o bônus
        /// para casas atacadas por peões inimigos.
        /// </remarks>
        /// <param name="pontuacao">Pontuação do lado branco ou preto</param>
        /// <param name="indice">Índice da casa do cavalo</param>
        private void AvaliaCavalo(Pontuacao pontuacao, int indice)
        {
            Fase -= Pontuacao.Fase.CAVALO;
            pontuacao.Inicial += Pontuacao.Material.CAVALO_INICIO;
            pontuacao.Inicial += Pontuacao.Tabela.CENTRALIZACAO[Defs.Converte12x12Para8x8(indice)];
            pontuacao.Final += Pontuacao.Material.CAVALO_FINAL;
            pontuacao.Final += Pontuacao.Tabela.CENTRALIZACAO[Defs.Converte12x12Para8x8(indice)];
        }


        /// <summary>
        /// Avalia o bispo.
        /// </summary>
        /// <remarks>
        /// Vai tentar centralizar o bispo.
        /// Outro item que pode ser adicionado é uma relação com os próprios peões, 
        /// não estando no quadrado da mesma cor do bispo.
        /// </remarks>
        /// <param name="pontuacao">Pontuação do lado branco ou preto</param>
        /// <param name="indice">Índice da casa do bispo</param>
        private void AvaliaBispo(Pontuacao pontuacao, int indice)
        {
            Fase -= Pontuacao.Fase.BISPO;
            pontuacao.Inicial += Pontuacao.Material.BISPO_INICIO;
            pontuacao.Inicial += Pontuacao.Tabela.CENTRALIZACAO[Defs.Converte12x12Para8x8(indice)];
            pontuacao.Final += Pontuacao.Material.BISPO_FINAL;
            pontuacao.Final += Pontuacao.Tabela.CENTRALIZACAO[Defs.Converte12x12Para8x8(indice)];
        }

        /// <summary>
        /// Avalia a torre.
        /// </summary>
        /// <remarks>
        /// Avalia a torre de cada lado do ponto de vista da cor, a visão do lado branco 
        /// é de baixo para cima e a visão do lado preto é de cima para baixo.
        /// Daremos um bônus para torre em coluna aberta e torre na 7ª fileira.
        /// Mobilidade e ataques a casas perto ao rei pode ser addicionada.
        /// </remarks>
        /// <param name="pontuacao">Pontuação do lado branco ou preto</param>
        /// <param name="indice">Índice da casa da torre</param>
        /// <param name="dadosTorre">Informação relativa à cor a ser avaliada.</param>
        private void AvaliaTorre(Pontuacao pontuacao, int indice, DadosTorre dadosTorre)
        {
            Fase -= Pontuacao.Fase.TORRE;

            pontuacao.Inicial += Pontuacao.Material.TORRE_INICIO;
            if (indice >= dadosTorre.IndiceFileira1 && indice < dadosTorre.IndiceFileira1 + 8) {
                int emFrente = indice + dadosTorre.DirecaoEmFrente;
                int peoesAmigos = 0;
                int peoesInimigos = 0;
                while (!Tabuleiro.BordaDoTabuleiro(emFrente)) {
                    if (Tabuleiro.ObtemPeca(emFrente) == dadosTorre.PeaoAmigo) peoesAmigos++;
                    if (Tabuleiro.ObtemPeca(emFrente) == dadosTorre.PeaoInimigo) peoesInimigos++;
                    emFrente += dadosTorre.DirecaoEmFrente;
                }
                if (peoesAmigos == 0 && peoesInimigos != 0) pontuacao.Inicial += Pontuacao.Torre.COLUNA_SEMI_ABERTA;
                if (peoesAmigos == 0 && peoesInimigos == 0) pontuacao.Inicial += Pontuacao.Torre.COLUNA_ABERTA;
            }

            pontuacao.Final += Pontuacao.Material.TORRE_FINAL;
            if (indice >= dadosTorre.IndiceFileira7 && indice < dadosTorre.IndiceFileira7 + 8) {
                for (int i = dadosTorre.IndiceFileira7; i < dadosTorre.IndiceFileira7 + 8; i++) {
                    if (Tabuleiro.ObtemPeca(i) == dadosTorre.PeaoInimigo) pontuacao.Final += Pontuacao.Torre.FILEIRA7_PEAO;
                }
            }
        }

        /// <summary>
        /// Avalia a Dama.
        /// </summary>
        /// <remarks>
        /// Mobilidade e ataques a casas perto ao rei pode ser addicionada.
        /// </remarks>
        /// <param name="pontuacao">Pontuação do lado branco ou preto</param>
        /// <param name="indice">Índice da casa da dama</param>
        private void AvaliaDama(Pontuacao pontuacao, int indice)
        {
            Fase -= Pontuacao.Fase.DAMA;
            pontuacao.Inicial += Pontuacao.Material.DAMA_INICIO;
            pontuacao.Final += Pontuacao.Material.DAMA_FINAL;
            pontuacao.Final += Pontuacao.Tabela.CENTRALIZACAO[Defs.Converte12x12Para8x8(indice)] / 2;
        }

        /// <summary>
        /// Avalia o Rei.
        /// </summary>
        /// <remarks>
        /// Apenas dá um bônus para peões na frente do rei, ou seja, escudo de peão (pawn shield).
        /// O que pode ser adicionado é uma penalidade para muitos peões inimigos perto do rei, o que 
        /// significa que um ataque de peão está se formando.
        /// Especialmente em termos de segurança do rei, se as peças inimigas estão atacando a área próxima ao rei, 
        /// pode ser um grande valor a este item. Em geral a segurança do rei é difícil de calibrar corretamente e 
        /// requer muitos testes.
        /// Além disso, algo a considerar é a presença da dama inimiga ao avaliar a segurança do rei. Sem a dama o
        /// valor da segurança do rei reduz drasticamente.
        /// </remarks>
        /// <param name="pontuacao">Pontuação do lado branco ou preto</param>
        /// <param name="indice">Índice da casa do rei</param>
        /// <param name="dadosRei">Informação relativa à cor a ser avaliada.</param>
        private void AvaliaRei(Pontuacao pontuacao, int indice, DadosRei dadosRei)
        {
            for (int i = 0; i < dadosRei.DirecaoEmFrente.Length; i++) {
                if (Tabuleiro.ObtemPeca(indice + i) == dadosRei.PeaoAmigo) pontuacao.Inicial += Pontuacao.Rei.PEAO_ESCUDO;
            }
            pontuacao.Inicial += dadosRei.TabelaInicio[Defs.Converte12x12Para8x8(indice)];
        }

        /// <summary>
        /// Calcule o resultado final e ajusta o placar de acordo com a fase do jogo.
        /// </summary>
        /// <remarks>
        /// Este cálculo cria um balanço para a pontuação de acordo com o número de peças no jogo. Assim, os valores que são 
        /// importantes para a abertura terão mais peso para mais peças e os valores para o final do jogo terão mais peso 
        /// com menos peças.
        /// </remarks>
        private int CalculaPontuacaoFinal()
        {
            if (Fase < 0) Fase = 0;

            int inicial = Branco.Inicial - Preto.Inicial;
            int final = Branco.Final - Preto.Final;

            return (inicial * (Pontuacao.Fase.TOTAL - Fase) + final * Fase) / Pontuacao.Fase.TOTAL;
        }
    }

    /// <summary>
    /// Pontuação da avaliação.
    /// </summary>
    /// <remarks>
    /// Valores usados durante a avaliação.
    /// Foram selectionados manualmente, então pode ser que algum possa ser melhorado. Atualemnte a maioria dos
    /// motores usa algum processo automático, como o "Texel tuning method". Texel é um forte motor de xadrez, 
    /// e seu autor popularizou esse método. 
    /// Observe que normalmente temos um valor para o início e fim do jogo.
    /// </remarks>
    public class Pontuacao
    {
        /// <summary>
        /// Números para representar fases do jogo.
        /// </summary>
        public class Fase
        {
            public const int PEAO = 0;
            public const int CAVALO = 1;
            public const int BISPO = 1;
            public const int TORRE = 2;
            public const int DAMA = 4;
            public const int TOTAL = PEAO * 16 + CAVALO * 4 + BISPO * 4 + TORRE * 4 + DAMA * 2;
        }

        /// <summary>
        /// Valor para cada peça.
        /// </summary>
        public class Material
        {
            public const int PEAO_INICIO = 90;
            public const int PEAO_FINAL = 90;
            public const int CAVALO_INICIO = 300;
            public const int CAVALO_FINAL = 300;
            public const int BISPO_INICIO = 330;
            public const int BISPO_FINAL = 330;
            public const int TORRE_INICIO = 500;
            public const int TORRE_FINAL = 500;
            public const int DAMA_INICIO = 900;
            public const int DAMA_FINAL = 900;
        }

        /// <summary>
        /// Tabela de valores com base na posição da peça.
        /// </summary>
        /// <remarks>
        /// Dá um valor à peça com base na sua posição no tabuleiro de xadrez. 
        /// É um bom valor geral para a peça, mas deve ser complementado por outro valor 
        /// específico da peça. E normalmente você tem tabelas por peças, aqui estamos 
        /// usando alguns valores genéricos.
        /// </remarks>
        public class Tabela
        {
            /// <summary>
            /// Tabela de peões brancos.
            /// </summary>
            /// <remarks>
            /// Incentivo para avançar.
            /// </remarks>
            public static readonly int[] PEAO_BRANCO =
            {
                  0,   0,   0,   0,   0,   0,   0,   0,
                128, 128, 128, 128, 128, 128, 128, 128,
                 64,  64,  64,  64,  64,  64,  64,  64,
                 32,  32,  32,  32,  32,  32,  32,  32,
                 16,  16,  16,  16,  16,  16,  16,  16,
                  8,   8,   8,   8,   8,   8,   8,   8,
                  0,   0,   0,   0,   0,   0,   0,   0,
                  0,   0,   0,   0,   0,   0,   0,   0,
            };
            /// <summary>
            /// Tabela de peões pretos.
            /// </summary>
            /// <remarks>
            /// Incentivo para avançar.
            /// </remarks>
            public static readonly int[] PEAO_PRETO =
            {
                  0,   0,   0,   0,   0,   0,   0,   0,
                  0,   0,   0,   0,   0,   0,   0,   0,
                  8,   8,   8,   8,   8,   8,   8,   8,
                 16,  16,  16,  16,  16,  16,  16,  16,
                 32,  32,  32,  32,  32,  32,  32,  32,
                 64,  64,  64,  64,  64,  64,  64,  64,
                128, 128, 128, 128, 128, 128, 128, 128,
                  0,   0,   0,   0,   0,   0,   0,   0,
            };
            /// <summary>
            /// Tabela genérica para centralização de peça.
            /// </summary>
            public static readonly int[] CENTRALIZACAO =
            {
                -50, -10, -10, -10, -10, -10, -10, -50,
                -10,   0,   0,   0,   0,   0,   0, -10,
                -10,   8,   8,   8,   8,   8,   8, -10,
                -10,  16,  16,  16,  16,  16,  16, -10,
                -10,  16,  16,  16,  16,  16,  16, -10,
                -10,   8,   8,   8,   8,   8,   8, -10,
                -10,   0,   0,   0,   0,   0,   0, -10,
                -50, -10, -10, -10, -10, -10, -10, -50,
            };
            /// <summary>
            /// Tabela para rei branco.
            /// </summary>
            /// <remarks>
            /// Incentiva o rei a ficar seguro na primeira fileira.
            /// </remarks>
            public static readonly int[] REI_BRANCO_INICIO =
            {
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                  0,  10,   0,   0,   0,   0,  10,   0,
            };
            /// <summary>
            /// Tabela para rei preto.
            /// </summary>
            /// <remarks>
            /// Incentiva o rei a ficar seguro na primeira fileira.
            /// </remarks>
            public static readonly int[] REI_PRETO_INICIO =
            {
                  0,  10,   0,   0,   0,   0,  10,   0,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
                -30, -30, -30, -30, -30, -30, -30, -30,
            };
        }

        /// <summary>
        /// Valores para peões.
        /// </summary>
        /// <remarks>
        /// Valoriza casas centrais.
        /// </remarks>
        public class Peao
        {
            public const int PEAO_CENTRAL_1 = 20;
            public const int PEAO_CENTRAL_2 = 10;
        }
        
        /// <summary>
        /// Valores para torres.
        /// </summary>
        /// <remarks>
        /// Basicamente valoriza torres em colunas abertas.
        /// </remarks>
        public class Torre
        {
            public const int COLUNA_SEMI_ABERTA = 5;
            public const int COLUNA_ABERTA = 10;
            public const int FILEIRA7_PEAO = 3;
            public const int FILEIRA7_REI = 10;
        }
        
        /// <summary>
        /// Valores para o rei.
        /// </summary>
        /// <remarks>
        /// Valoriza peões em frente ao rei (escudo).
        /// </remarks>
        public class Rei
        {
            public const int PEAO_ESCUDO = 6;
        }

        /// <summary>
        /// Valor total inicial da avaliação.
        /// </summary>
        public int Inicial;
        
        /// <summary>
        /// Valor total final da avaliação.
        /// </summary>
        public int Final;

        /// <summary>
        /// Inicializa pontuação ao iniciar uma nova avaliação.
        /// </summary>
        public void ZeraPontuacao()
        {
            Inicial = Final = 0;
        }
    }
}

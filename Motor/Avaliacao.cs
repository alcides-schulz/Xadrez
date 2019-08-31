using System;

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
    /// Valores positivos são dados a termos que são considerados bônus e valores negativos penalidades. Já a
    /// importância é definida pelo valor atribuído, quanto mais próximo a zero menor a importância do termo.
    /// Com bônus e/ou penalidades para cada lado estima-se um valor resultante (que indica quem está melhor). 
    /// 
    /// Cada termo avaliado tem um custo computacional na identificação do comportamento no tabuleiro, por isso
    /// é melhor usar termos que acontecem com mais frequência ou com bastante importância, já que usar um termo muito
    /// específico e com pouca importância pode afetar negativamente o desempenho. 
    /// 
    /// Os termos mais comuns em uma avaliação são:
    /// - material - Cada peça tem seu valor, podemos definir que uma Rainha vale mais que um Cavalo.
    /// - segurança do rei - Bonus para peças atacando casas próximas ao Rei, possíveis cheques, etc.
    /// - estrutura do peão - Penalidades para peões isolados, peões duplicados, etc. Bônus para peões conectados, 
    ///   passados, etc.
    /// - mobilidade da peça - Quanto mais movimentos disponíveis para cada peça melhor.
    /// 
    /// Há muitos outros termos, tente colocar no seu motor a maior quantidade de termos que melhoram sua performance.
    /// É importante automatizar a atribuição de valores dos termos. É muito difícil selecionar manualmente valores
    /// corretos para cada termo e já existem alguns processos de ajuste automático. O "Método de afinação Texel"
    /// ("Texel tuning method") é bem simples e popular. Vários motores já utilizaram este método com sucesso, como
    /// por exemplo, o Tucano e o Pirarucu.
    /// 
    /// A avaliação que vamos mostrar é muito simples e pode ser melhorada pela adição de:
    /// - mobilidade das peças: para o cavalo, bispo, torre e dama, conte quantas casas podem alcançar. Você também pode 
    ///   adicionar o número de peças próprias que defende ou o número de peças inimigas que ataca. Isso influencia o
    ///   estilo de jogo, pode tentar atacar mais ou defender mais. Isso é algo a ser testado.
    /// - Estrutura de peão: penalize peões isolados, peões duplos e dê bônus para peões passados. 
    /// 
    /// Para cada mudança feita na avaliação é necessário testar se teve um impacto positivo, afinal só queremos usar
    /// termos que dão uma melhoria na performance. Não gaste muito tempo procurando valores perfeitos para cada termo,
    /// use valores que melhoram a performance. É preferível testar várias modificações diferentes do que ficar
    /// muito tempo testando pequenas otimizações, já que os valores ideais são específicos para cada conjunto de
    /// termos.
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
        /// Valor da posição que será retornado.
        /// </summary>
        public int PontuacaoFinal;

        /// <summary>
        /// Parâmetros de avaliação da torre.
        /// </summary>
        /// <remarks>
        /// A avaliação da torre precisa especificar parâmetros dependendo do lado avaliado. 
        /// Ao criar um conjunto de parâmetros para cada lado, é fácil reutilizar o método de avaliação da torre.
        /// Existem outras maneiras de fazer essa avaliação, isso é apenas uma idéia.
        /// </remarks>
        private class DadosTorre
        {
            public Pontuacao Pontuacao;
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
            public int[] DirecaoEmFrente;
            public Peca PeaoAmigo;
            public int[] TabelaInicio;
        }

        private DadosTorre DadosTorreBranca = new DadosTorre();
        private DadosTorre DadosTorrePreta = new DadosTorre();
        private DadosRei DadosReiBranco = new DadosRei();
        private DadosRei DadosReiPreto = new DadosRei();

        /// <summary>
        /// Cria nova avaliação e inicializa parâmetros.
        /// </summary>
        /// <param name="tabuleiro">Tabuleiro a ser avaliado.</param>
        public Avaliacao(Tabuleiro tabuleiro)
        {
            Tabuleiro = tabuleiro;

            DadosTorreBranca.Pontuacao = Branco;
            DadosTorreBranca.IndiceFileira1 = (int)Defs.INDICE.A1;
            DadosTorreBranca.IndiceFileira7 = (int)Defs.INDICE.A7;
            DadosTorreBranca.DirecaoEmFrente = Defs.POSICAO_NORTE;
            DadosTorreBranca.PeaoAmigo = Peca.PeaoBranco;
            DadosTorreBranca.PeaoInimigo = Peca.PeaoPreto;

            DadosTorrePreta.Pontuacao = Preto;
            DadosTorrePreta.IndiceFileira1 = (int)Defs.INDICE.A8;
            DadosTorrePreta.IndiceFileira7 = (int)Defs.INDICE.A2;
            DadosTorrePreta.DirecaoEmFrente = Defs.POSICAO_SUL;
            DadosTorrePreta.PeaoAmigo = Peca.PeaoPreto;
            DadosTorrePreta.PeaoInimigo = Peca.PeaoBranco;

            DadosReiBranco.DirecaoEmFrente = new int[] { Defs.POSICAO_NOROESTE, Defs.POSICAO_NORTE, Defs.POSICAO_NORDESTE };
            DadosReiBranco.PeaoAmigo = Peca.PeaoBranco;
            DadosReiBranco.TabelaInicio = Pontuacao.Tabela.REI_BRANCO_INICIO;

            DadosReiPreto.DirecaoEmFrente = new int[] { Defs.POSICAO_SUDOESTE, Defs.POSICAO_SUL, Defs.POSICAO_SUDESTE };
            DadosReiPreto.PeaoAmigo = Peca.PeaoPreto;
            DadosReiPreto.TabelaInicio = Pontuacao.Tabela.REI_PRETO_INICIO;
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
            CalculaPontuacaoFinal();

            return Tabuleiro.CorJogar.Multiplicador() * PontuacaoFinal;
        }

        /// <summary>
        /// Zera os valores para avaliação.
        /// </summary>
        private void PreparaAvaliacao()
        {
            Fase = Pontuacao.Fase.TOTAL;
            Branco.ZeraPontuacao();
            Preto.ZeraPontuacao();
            PontuacaoFinal = 0;
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
                            AvaliaPeaoBranco(indice); 
                            break;
                        case Peca.CavaloBranco:
                            AvaliaCavalo(Branco, indice);
                            break;
                        case Peca.BispoBranco:
                            AvaliaBispo(Branco, indice);
                            break;
                        case Peca.TorreBranca:
                            AvaliaTorre(Branco, indice, DadosTorreBranca);
                            break;
                        case Peca.DamaBranca:
                            AvaliaDama(Branco, indice);
                            break;
                        case Peca.ReiBranco:
                            AvaliaRei(Branco, indice, DadosReiBranco);
                            break;
                        case Peca.PeaoPreto:
                            AvaliaPeaoPreto(indice); 
                            break;
                        case Peca.CavaloPreto:
                            AvaliaCavalo(Preto, indice);
                            break;
                        case Peca.BispoPreto:
                            AvaliaBispo(Preto, indice);
                            break;
                        case Peca.TorrePreta:
                            AvaliaTorre(Preto, indice, DadosTorreBranca);
                            break;
                        case Peca.DamaPreta:
                            AvaliaDama(Preto, indice);
                            break;
                        case Peca.ReiPreto:
                            AvaliaRei(Preto, indice, DadosReiBranco);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Avalia peões brancos.
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
        /// <param name="indice">Índice da casa do peão</param>
        private void AvaliaPeaoBranco(int indice)
        {
            Fase -= Pontuacao.Fase.PEAO;
            Branco.Inicial += Pontuacao.Material.PEAO_INICIO;
            Branco.Inicial += Pontuacao.Tabela.PEAO_BRANCO[Defs.Converte12x12Para8x8(indice)];
            if (indice == (int)Defs.INDICE.D4 || indice == (int)Defs.INDICE.E4) Branco.Inicial += Pontuacao.Peao.PEAO_CENTRAL_1;
            if (indice == (int)Defs.INDICE.D3 || indice == (int)Defs.INDICE.E3) Branco.Inicial += Pontuacao.Peao.PEAO_CENTRAL_2;
            Branco.Final += Pontuacao.Material.PEAO_FINAL;
            Branco.Final += Pontuacao.Tabela.PEAO_BRANCO[Defs.Converte12x12Para8x8(indice)];
        }

        /// <summary>
        /// Avalia peões pretos.
        /// </summary>
        /// <see cref="AvaliaPeaoBranco(int)"/>
        /// <param name="indice">Índice da casa do peão</param>
        private void AvaliaPeaoPreto(int indice)
        {
            Fase -= Pontuacao.Fase.PEAO;
            Preto.Inicial += Pontuacao.Material.PEAO_INICIO;
            Preto.Inicial += Pontuacao.Tabela.PEAO_PRETO[Defs.Converte12x12Para8x8(indice)];
            if (indice == (int)Defs.INDICE.D5 || indice == (int)Defs.INDICE.E5) Preto.Inicial += Pontuacao.Peao.PEAO_CENTRAL_1;
            if (indice == (int)Defs.INDICE.D6 || indice == (int)Defs.INDICE.E6) Preto.Inicial += Pontuacao.Peao.PEAO_CENTRAL_2;
            Preto.Final += Pontuacao.Material.PEAO_FINAL;
            Preto.Final += Pontuacao.Tabela.PEAO_PRETO[Defs.Converte12x12Para8x8(indice)];
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
        /// <param name="dados_torre">Informação relativa à cor a ser avaliada.</param>
        private void AvaliaTorre(Pontuacao pontuacao, int indice, DadosTorre dados_torre)
        {
            Fase -= Pontuacao.Fase.TORRE;

            pontuacao.Inicial += Pontuacao.Material.TORRE_INICIO;
            if (indice >= dados_torre.IndiceFileira1 && indice < dados_torre.IndiceFileira1 + 8) {
                int em_frente = indice + dados_torre.DirecaoEmFrente;
                int peoes_amigos = 0;
                int peoes_inimigos = 0;
                while (!Tabuleiro.BordaDoTabuleiro(em_frente)) {
                    if (Tabuleiro.ObtemPeca(em_frente) == dados_torre.PeaoAmigo) peoes_amigos++;
                    if (Tabuleiro.ObtemPeca(em_frente) == dados_torre.PeaoInimigo) peoes_inimigos++;
                    em_frente += dados_torre.DirecaoEmFrente;
                }
                if (peoes_amigos == 0 && peoes_inimigos != 0) pontuacao.Inicial += Pontuacao.Torre.COLUNA_SEMI_ABERTA;
                if (peoes_amigos == 0 && peoes_inimigos == 0) pontuacao.Inicial += Pontuacao.Torre.COLUNA_ABERTA;
            }

            pontuacao.Final += Pontuacao.Material.TORRE_FINAL;
            if (indice >= dados_torre.IndiceFileira7 && indice < dados_torre.IndiceFileira7 + 8) {
                for (int i = dados_torre.IndiceFileira7; i < dados_torre.IndiceFileira7 + 8; i++) {
                    if (Tabuleiro.ObtemPeca(i) == dados_torre.PeaoInimigo) pontuacao.Final += Pontuacao.Torre.FILEIRA7_PEAO;
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
        /// <param name="dados_rei">Informação relativa à cor a ser avaliada.</param>
        private void AvaliaRei(Pontuacao pontuacao, int indice, DadosRei dados_rei)
        {
            for (int i = 0; i < dados_rei.DirecaoEmFrente.Length; i++) {
                if (Tabuleiro.ObtemPeca(indice + i) == dados_rei.PeaoAmigo) pontuacao.Inicial += Pontuacao.Rei.PEAO_ESCUDO;
            }
            pontuacao.Inicial += dados_rei.TabelaInicio[Defs.Converte12x12Para8x8(indice)];
        }

        /// <summary>
        /// Calcule o resultado final e ajusta o placar de acordo com a fase do jogo.
        /// </summary>
        /// <remarks>
        /// Este cálculo cria um balanço para a pontuação de acordo com o número de peças no jogo. Assim, os valores que são 
        /// importantes para a abertura terão mais peso para mais peças e os valores para o final do jogo terão mais peso 
        /// com menos peças.
        /// </remarks>
        private void CalculaPontuacaoFinal()
        {
            if (Fase < 0) Fase = 0;

            int inicial = Branco.Inicial - Preto.Inicial;
            int final = Branco.Final - Preto.Final;

            PontuacaoFinal = ((inicial * (Pontuacao.Fase.TOTAL - Fase)) + (final * Fase)) / Pontuacao.Fase.TOTAL;
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

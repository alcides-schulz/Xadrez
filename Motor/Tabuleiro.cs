using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Enxadrista
{
    /// <summary>
    ///     Tabuleiro de Xadrez:
    ///     mantém informações da posição,
    ///     gera lista de movimentos,
    ///     faz e desfaz movimentos,
    ///     verifica se um movimento feito é legal.
    /// </summary>
    /// <remarks>
    ///     É provável que você comece a programar pelo tabuleiro de xadrez.
    ///     Escolha a representação que deseja usar e certifique-se de que a geração de movimento
    ///     e a execução do movimento estão funcionando sem erros.
    ///     No modulo de unidade de testes, adicionei algumas técnicas de teste.
    ///     A principal técnica é chamada "perft". Basicamente, você tem uma posição de xadrez
    ///     e o número de movimentos, para que você possa comparar com o seu gerador de movimentos.
    ///     Por exemplo: a partir da posição inicial você tem 20 movimentos possíveis, se você considerar
    ///     a resposta do lado preto, então você tem 400 movimentos.
    ///     Em si, a geração de movimentos já é uma tarefa muito desafiadora, há algumas situações
    ///     propensas a erros quando você tem en-passant, promoções, castelo, etc.
    ///     Seja paciente, recomendo não iniciar outros módulos até que o seu gerador de movimento
    ///     esteja 100% livre de erros.
    ///     Importante: escolha uma representação simples, provavelmente você irá reescrever seu
    ///     programa de xadrez pelo menos uma vez. Depende de quanto você vai gostar :)
    ///     Existem várias maneiras de representar o tabuleiro de xadrez para um motor.
    ///     - matriz de 8x8 bytes: simples, mas não funciona bem em comparação com as opções abaixo.
    ///     - matriz de bytes usando lógica 0x88: Usa uma matriz de 128 bytes que, devido à propriedades binarias,
    ///     apresenta uma série de atalhos ao gerar movimentos ou verificar posições. Essa representação é
    ///     muito interessante.
    ///     - matriz de 12 x 12. A idéia é ter uma borda ao redor do tabuleiro de xadrez que ajude na
    ///     geração de movimentos. Por ser simples, usaremos essa representação neste programa.
    ///     - bitboards: estrutura mais utilizada por programas competitivos. Baseia-se no princípio
    ///     de que uma variável de 64 bits pode ser usada para representar as 64 posições do tabuleiro.
    ///     O uso das funções para manipulação de bits pode ser feito em poucas instruções da CPU e isso
    ///     ajuda em muitas funções do programa, como geração de movimento, testes para a localização de
    ///     outras peças, etc.
    ///     Fazer um programa usando bitboard é um grande desafio.
    /// </remarks>
    public class Tabuleiro
    {
        /// <summary>
        ///     Chave Zobrist para posição atual.
        /// </summary>
        /// <remarks>
        ///     Este chave vai servir como identificador único da posição atual.
        ///     Será usado para validar a regra de 50 movimentos e regra de 3 repetições.
        ///     A maioria dos motores de xadrez usam esta técnica (zobrist keys).
        ///     Essa chave é, de fato, um número, que é calculado sempre que uma nova jogada é feita.
        ///     Veja a classe Zobrist.cs.
        /// </remarks>
        /// <see cref="Zobrist" />
        public ulong Chave;

        /// <summary>
        ///     Contador dos movimentos feitos, incrementado após lance da cor preta.
        /// </summary>
        public int ContadorMovimentos;

        /// <summary>
        ///     Contador da regra dos 50 movimentos.
        /// </summary>
        public int ContadorRegra50;

        /// <summary>
        ///     Cor na vez de jogar: branco ou preto.
        /// </summary>
        public Cor CorJogar;

        /// <summary>
        ///     História dos movimentos feitos
        /// </summary>
        private readonly Historia[] Historia = new Historia[Defs.HISTORIA_MAXIMA];

        /// <summary>
        ///     Índice da casa do movimento en-passant, ou 0 se não estiver disponível.
        /// </summary>
        public int IndiceEnPassant;

        /// <summary>
        ///     Índice atual para história e é atualizado sempre que um movimento é feito ou desfeito.
        /// </summary>
        public int IndiceHistoria;

        /// <summary>
        ///     Posição do rei branco.
        /// </summary>
        public int IndiceReiBranco;

        /// <summary>
        ///     Posição do rei preto.
        /// </summary>
        public int IndiceReiPreto;

        /// <summary>
        ///     Casas e bordas do tabuleiro
        /// </summary>
        /// <remarks>
        ///     Vamos usar como uma matriz de 12 por 12 sbytes. Veja a representação abaixo.
        ///     As bordas são usadas durante a geração de movimentos para indicar os limites do tabuleiro.
        ///     <code>
        /// XXXXXXXXXXXX
        /// XXXXXXXXXXXX
        /// XX--------XX
        /// XX--------XX
        /// XX--------XX
        /// XX--------XX
        /// XX--------XX
        /// XX--------XX
        /// XX--------XX
        /// XX--------XX
        /// XXXXXXXXXXXX
        /// XXXXXXXXXXXX
        /// </code>
        /// </remarks>
        private readonly Peca[] Quadrados = new Peca[Defs.INDICE_MAXIMO];

        /// <summary>
        ///     Indica se o roque E1C1 pode ser feito.
        /// </summary>
        public bool RoqueE1C1;

        /// <summary>
        ///     Indica se o roque E1G1 pode ser feito.
        /// </summary>
        /// <remarks>
        ///     King ou Torre não se movimentaram, e é atualizado em cada movimento.
        /// </remarks>
        public bool RoqueE1G1;

        /// <summary>
        ///     Indica se o roque E8C8 pode ser feito.
        /// </summary>
        public bool RoqueE8C8;

        /// <summary>
        ///     Indica se o roque E8G8 pode ser feito.
        /// </summary>
        public bool RoqueE8G8;

        /// <summary>
        ///     Construtor: Inicializa dados e inicia um novo jogo a partir da posição inicial.
        /// </summary>
        public Tabuleiro()
        {
            for (var fileira = 0; fileira < 12; fileira++)
            for (var coluna = 0; coluna < 12; coluna++)
                if (fileira < Defs.PRIMEIRA_FILEIRA || fileira >= Defs.ULTIMA_FILEIRA ||
                    coluna < Defs.PRIMEIRA_COLUNA || coluna >= Defs.ULTIMA_COLUNA)
                    Quadrados[fileira * Defs.NUMERO_COLUNAS + coluna] = Peca.Borda;
                else
                    Quadrados[fileira * Defs.NUMERO_COLUNAS + coluna] = Peca.Nenhuma;

            for (var i = 0; i < Historia.Length; i++) Historia[i] = new Historia();

            NovaPartida(Defs.FEN_POSICAO_INICIAL);
        }

        /// <summary>
        ///     Cria uma nova partida a partir the uma descricao "fen".
        /// </summary>
        /// <see cref="FEN" />
        /// <param name="fen">Cadeia de characteres representando uma posição no formato FEN.</param>
        public void NovaPartida(string fen)
        {
            for (var fileira = Defs.PRIMEIRA_FILEIRA; fileira < Defs.ULTIMA_FILEIRA; fileira++)
            for (var coluna = Defs.PRIMEIRA_COLUNA; coluna < Defs.ULTIMA_COLUNA; coluna++)
                Quadrados[fileira * Defs.NUMERO_COLUNAS + coluna] = Peca.Nenhuma;

            CorJogar = Cor.Nenhuma;
            RoqueE1G1 = false;
            RoqueE1C1 = false;
            RoqueE8G8 = false;
            RoqueE8C8 = false;
            IndiceEnPassant = 0;
            ContadorRegra50 = 0;
            ContadorMovimentos = 0;
            IndiceReiBranco = 0;
            IndiceReiPreto = 0;
            IndiceHistoria = 0;
            FEN.ConverteFenParaTabuleiro(fen, this);
            Chave = Zobrist.ObtemChave(this);
        }

        /// <summary>
        ///     Retorna uma string FEN representando a posição atual.
        /// </summary>
        /// <returns>string FEN</returns>
        public string ObtemFEN()
        {
            return FEN.ConverteTabuleiroParaFEN(this);
        }

        /// <summary>
        ///     Gera os movimentos possíveis para o lado que está na vez.
        /// </summary>
        /// <remarks>
        ///     Não verifica se o rei é deixado em cheque, isso será feito mais tarde ao executar o movimento.
        ///     A idéia é que você não executa esta tarefa agora e, eventualmente, alguns movimentos não serão
        ///     executados durante a pesquisa alfa/beta. Pode economizar algum tempo e melhorar o desempenho.
        ///     Algumas melhorias que podem ser feitas nesse processo. A maioria dos programas fortes faz isso.
        ///     - Separar a geração de movimentos de captura e não captura. Isso pode ser usado na busca de estado quiescente,
        ///     onde você precisa apenas de movimentos de capturas e promoção (veja o modulo pesquisa quiescente).
        ///     Em geral é melhor pesquisar movimentos de captura primeiro, devido à natureza da pesquisa alfa-beta.
        ///     Você pode separar em dois métodos:
        ///     - gerar movimentos de captura (inclui movimentos de promoção)
        ///     - gerar movimentos tranquilos (não captura)
        ///     - Quando o rei está no xeque, você só precisa gerar os movimentos que tiram o rei do xeque.
        ///     Em geral, é um número muito menor de movimentos a serem gerados.
        ///     - Manter uma lista para cada tipo de peça, então não pesquisamos todo o tabuleiro de xadrez
        ///     para localizar as peças.
        ///     - Em vez de usar um objeto List<>, devemos usar uma lista estática. Uma vez que geramos milhões de movimentos
        ///     durante a pesquisa, isso pode ser lento devido à alocação e liberacão de memória.
        ///     Esta lista pode ser alocada no início do programa e ajuda na performace.
        ///     - Também criar objetos "Movimento" pode ser lento em termos de gerenciamento de memória.
        ///     Normalmente um movimento é representado por uma variável "integer", onde você codifica as coordenadas e a
        ///     memória não é alocada dinamicamente toda vez que você cria um novo objeto (usando o comando "new").
        /// </remarks>
        /// <see cref="Pesquisa" />
        /// <returns>Lista de movimentos para a posição atual.</returns>
        public List<Movimento> GeraMovimentos()
        {
            var lista = new List<Movimento>();

            for (var fileira = Defs.PRIMEIRA_FILEIRA; fileira < Defs.ULTIMA_FILEIRA; fileira++)
            for (var coluna = Defs.PRIMEIRA_COLUNA; coluna < Defs.ULTIMA_COLUNA; coluna++)
            {
                var indice = fileira * Defs.NUMERO_COLUNAS + coluna;
                var item = Quadrados[indice];
                if (item == Peca.Nenhuma) continue;
                if (CorJogar == Cor.Branca)
                    switch (item)
                    {
                        case Peca.PeaoBranco:
                            GeraMovimentosPeaoBranco(lista, indice);
                            break;
                        case Peca.CavaloBranco:
                            GeraMovimentosCavalo(lista, indice);
                            break;
                        case Peca.BispoBranco:
                            GeraMovimentosBispo(lista, indice);
                            break;
                        case Peca.TorreBranca:
                            GeraMovimentosTorre(lista, indice);
                            break;
                        case Peca.DamaBranca:
                            GeraMovimentosBispo(lista, indice);
                            GeraMovimentosTorre(lista, indice);
                            break;
                        case Peca.ReiBranco:
                            GeraMovimentosRei(lista, indice);
                            break;
                    }
                else
                    switch (item)
                    {
                        case Peca.PeaoPreto:
                            GeraMovimentosPeaoPreto(lista, indice);
                            break;
                        case Peca.CavaloPreto:
                            GeraMovimentosCavalo(lista, indice);
                            break;
                        case Peca.BispoPreto:
                            GeraMovimentosBispo(lista, indice);
                            break;
                        case Peca.TorrePreta:
                            GeraMovimentosTorre(lista, indice);
                            break;
                        case Peca.DamaPreta:
                            GeraMovimentosBispo(lista, indice);
                            GeraMovimentosTorre(lista, indice);
                            break;
                        case Peca.ReiPreto:
                            GeraMovimentosRei(lista, indice);
                            break;
                    }
            }

            return lista;
        }

        /// <summary>
        ///     Gera movimentos do peão branco.
        /// </summary>
        /// <remarks>
        ///     - Captura e captura para promoção
        ///     - Captura en-passant
        ///     - Avanço simples e avanço de dois quadrados no primeiro movimento
        ///     - Promoções para dama, torre, bispo e cavalo
        /// </remarks>
        /// <param name="lista">Lista que recebe os movimentos</param>
        /// <param name="indice_origem">Posição do peão</param>
        private void GeraMovimentosPeaoBranco(List<Movimento> lista, int indice_origem)
        {
            var indice_captura_1 = indice_origem + Defs.POSICAO_NORDESTE;
            if (CasaComPecaPreta(indice_captura_1))
            {
                if (Defs.FileiraPromocaoBranco(indice_captura_1))
                {
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1], Peca.DamaBranca));
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1], Peca.TorreBranca));
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1], Peca.BispoBranco));
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1], Peca.CavaloBranco));
                }
                else
                {
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1]));
                }
            }
            else
            {
                if (indice_captura_1 == IndiceEnPassant)
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_1,
                        indice_captura_1 + Defs.POSICAO_SUL));
            }

            var indice_captura_2 = indice_origem + Defs.POSICAO_NOROESTE;
            if (CasaComPecaPreta(indice_captura_2))
            {
                if (Defs.FileiraPromocaoBranco(indice_captura_2))
                {
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2], Peca.DamaBranca));
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2], Peca.TorreBranca));
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2], Peca.BispoBranco));
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2], Peca.CavaloBranco));
                }
                else
                {
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2]));
                }
            }
            else
            {
                if (indice_captura_2 == IndiceEnPassant)
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_captura_2,
                        indice_captura_2 + Defs.POSICAO_SUL));
            }

            var indice_movimento = indice_origem + Defs.POSICAO_NORTE;
            if (CasaVazia(indice_movimento))
            {
                if (Defs.FileiraPromocaoBranco(indice_movimento))
                {
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_movimento, Peca.Nenhuma,
                        Peca.DamaBranca));
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_movimento, Peca.Nenhuma,
                        Peca.TorreBranca));
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_movimento, Peca.Nenhuma,
                        Peca.BispoBranco));
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_movimento, Peca.Nenhuma,
                        Peca.CavaloBranco));
                }
                else
                {
                    lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_movimento));
                    if (Defs.PrimeiraFileiraPeaoBranco(indice_origem))
                    {
                        var indice_movimento_2 = indice_movimento + Defs.POSICAO_NORTE;
                        if (CasaVazia(indice_movimento_2))
                            lista.Add(new Movimento(Peca.PeaoBranco, indice_origem, indice_movimento_2));
                    }
                }
            }
        }

        /// <summary>
        ///     Gera movimentos do peão preto.
        /// </summary>
        /// <remarks>
        ///     - Captura e captura para promoção
        ///     - Captura en-passant
        ///     - Avanço simples e avanço de dois quadrados no primeiro movimento
        ///     - Promoções para dama, torre, bispo e cavalo
        /// </remarks>
        /// <param name="lista">Lista que recebe os movimentos</param>
        /// <param name="indice_origem">Posição do peão</param>
        private void GeraMovimentosPeaoPreto(List<Movimento> lista, int indice_origem)
        {
            var indice_captura_1 = indice_origem + Defs.POSICAO_SUDESTE;
            if (CasaComPecaBranca(indice_captura_1))
            {
                if (Defs.FileiraPromocaoPreto(indice_captura_1))
                {
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1], Peca.DamaPreta));
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1], Peca.TorrePreta));
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1], Peca.BispoPreto));
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1], Peca.CavaloPreto));
                }
                else
                {
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_1,
                        Quadrados[indice_captura_1]));
                }
            }
            else
            {
                if (indice_captura_1 == IndiceEnPassant)
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_1,
                        indice_captura_1 + Defs.POSICAO_NORTE));
            }

            var indice_captura_2 = indice_origem + Defs.POSICAO_SUDOESTE;
            if (CasaComPecaBranca(indice_captura_2))
            {
                if (Defs.FileiraPromocaoPreto(indice_captura_2))
                {
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2], Peca.DamaPreta));
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2], Peca.TorrePreta));
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2], Peca.BispoPreto));
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2], Peca.CavaloPreto));
                }
                else
                {
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_2,
                        Quadrados[indice_captura_2]));
                }
            }
            else
            {
                if (indice_captura_2 == IndiceEnPassant)
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_captura_2,
                        indice_captura_2 + Defs.POSICAO_NORTE));
            }

            var indice_movimento = indice_origem + Defs.POSICAO_SUL;
            if (CasaVazia(indice_movimento))
            {
                if (Defs.FileiraPromocaoPreto(indice_movimento))
                {
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_movimento, Peca.Nenhuma,
                        Peca.DamaPreta));
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_movimento, Peca.Nenhuma,
                        Peca.TorrePreta));
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_movimento, Peca.Nenhuma,
                        Peca.BispoPreto));
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_movimento, Peca.Nenhuma,
                        Peca.CavaloPreto));
                }
                else
                {
                    lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_movimento));
                    if (Defs.PrimeiraFileiraPeaoPreto(indice_origem))
                    {
                        var indice_movimento_2 = indice_movimento + Defs.POSICAO_SUL;
                        if (CasaVazia(indice_movimento_2))
                            lista.Add(new Movimento(Peca.PeaoPreto, indice_origem, indice_movimento_2));
                    }
                }
            }
        }

        /// <summary>
        ///     Gera possíveis movimentos de cavalos.
        /// </summary>
        /// <param name="lista">Lista que recebe os movimentos</param>
        /// <param name="indice_origem">Posição do cavalo</param>
        private void GeraMovimentosCavalo(List<Movimento> lista, int indice_origem)
        {
            foreach (var movimento in Defs.Movimentos.CAVALO)
                GeraMovimento(lista, indice_origem, indice_origem + movimento);
        }

        /// <summary>
        ///     Gera possíveis movimentos de torre. Também usado para gerar movimentos da dama.
        /// </summary>
        /// <param name="lista">Lista que recebe os movimentos</param>
        /// <param name="indice_origem">Posição da torre ou dama</param>
        private void GeraMovimentosTorre(List<Movimento> lista, int indice_origem)
        {
            foreach (var movimento in Defs.Movimentos.TORRE) GeraMovimentosNaDirecao(lista, indice_origem, movimento);
        }

        /// <summary>
        ///     Gera possíveis movimentos de bispo. Também usado para gerar movimentos da dama.
        /// </summary>
        /// <param name="lista">Lista que recebe os movimentos</param>
        /// <param name="indice_origem">Posição do bispo ou dama</param>
        private void GeraMovimentosBispo(List<Movimento> lista, int indice_origem)
        {
            foreach (var movimento in Defs.Movimentos.BISPO) GeraMovimentosNaDirecao(lista, indice_origem, movimento);
        }

        /// <summary>
        ///     Gera possíveis movimentos do rei. Também gera os movimentos do roque quando possível.
        /// </summary>
        /// <param name="lista">Lista que recebe os movimentos</param>
        /// <param name="indice_origem">Posição do rei</param>
        private void GeraMovimentosRei(List<Movimento> lista, int indice_origem)
        {
            foreach (var movimento in Defs.Movimentos.REI)
                GeraMovimento(lista, indice_origem, indice_origem + movimento);

            if (CorJogar == Cor.Branca)
            {
                if (RoqueE1G1 && PodeGerarRoque((int) Defs.INDICE.F1, (int) Defs.INDICE.G1, (int) Defs.INDICE.H1,
                        Peca.TorreBranca))
                    lista.Add(new Movimento(Peca.ReiBranco, indice_origem, (int) Defs.INDICE.G1));
                if (RoqueE1C1 && PodeGerarRoque((int) Defs.INDICE.B1, (int) Defs.INDICE.D1, (int) Defs.INDICE.A1,
                        Peca.TorreBranca))
                    lista.Add(new Movimento(Peca.ReiBranco, indice_origem, (int) Defs.INDICE.C1));
            }
            else
            {
                if (RoqueE8G8 && PodeGerarRoque((int) Defs.INDICE.F8, (int) Defs.INDICE.G8, (int) Defs.INDICE.H8,
                        Peca.TorrePreta)) lista.Add(new Movimento(Peca.ReiPreto, indice_origem, (int) Defs.INDICE.G8));
                if (RoqueE8C8 && PodeGerarRoque((int) Defs.INDICE.B8, (int) Defs.INDICE.D8, (int) Defs.INDICE.A8,
                        Peca.TorrePreta)) lista.Add(new Movimento(Peca.ReiPreto, indice_origem, (int) Defs.INDICE.C8));
            }
        }

        private bool PodeGerarRoque(int casa_vazia_inicio, int casa_vazia_final, int casa_torre, Peca peca_torre)
        {
            if (ObtemPeca(casa_torre) != peca_torre) return false;
            for (var casa_vazia = casa_vazia_inicio; casa_vazia <= casa_vazia_final; casa_vazia++)
                if (!CasaVazia(casa_vazia))
                    return false;
            return true;
        }

        /// <summary>
        ///     Função de utilidade que gera movimentos em uma determinada direção.
        ///     Irá em uma direção até chegar à borda do tabuleiro, e pode gerar movimentos ou capturas.
        /// </summary>
        /// <param name="lista">Lista que recebe os movimentos</param>
        /// <param name="indice_origem">Índice da peça que vai mover</param>
        /// <param name="direcao">valor do deslocamento indicando a direção, examplo: Norte, Sul, etc.</param>
        private void GeraMovimentosNaDirecao(List<Movimento> lista, int indice_origem, int direcao)
        {
            for (var indice_destino = indice_origem + direcao;
                !BordaDoTabuleiro(indice_destino);
                indice_destino += direcao)
            {
                if (CasaVazia(indice_destino))
                {
                    lista.Add(new Movimento(Quadrados[indice_origem], indice_origem, indice_destino));
                    continue;
                }

                if (CorJogar == Cor.Branca && CasaComPecaPreta(indice_destino))
                    lista.Add(new Movimento(Quadrados[indice_origem], indice_origem, indice_destino,
                        Quadrados[indice_destino]));
                if (CorJogar == Cor.Preta && CasaComPecaBranca(indice_destino))
                    lista.Add(new Movimento(Quadrados[indice_origem], indice_origem, indice_destino,
                        Quadrados[indice_destino]));
                break;
            }
        }

        /// <summary>
        ///     Função de utilidade que gera um movimento ou captura.
        /// </summary>
        /// <param name="lista">Lista que recebe os movimentos</param>
        /// <param name="indice_origem">Índice da peça que vai mover</param>
        /// <param name="indice_destino">Índice da posição destino.</param>
        private void GeraMovimento(List<Movimento> lista, int indice_origem, int indice_destino)
        {
            Debug.Assert(indice_origem >= 0 && indice_origem < Defs.INDICE_MAXIMO);
            Debug.Assert(indice_destino >= 0 && indice_destino < Defs.INDICE_MAXIMO);
            if (CorJogar == Cor.Branca && CasaComPecaPreta(indice_destino))
            {
                lista.Add(new Movimento(ObtemPeca(indice_origem), indice_origem, indice_destino,
                    ObtemPeca(indice_destino)));
                return;
            }

            if (CorJogar == Cor.Preta && CasaComPecaBranca(indice_destino))
            {
                lista.Add(new Movimento(ObtemPeca(indice_origem), indice_origem, indice_destino,
                    ObtemPeca(indice_destino)));
                return;
            }

            if (CasaVazia(indice_destino))
                lista.Add(new Movimento(ObtemPeca(indice_origem), indice_origem, indice_destino));
        }

        /// <summary>
        ///     Executa o movimento no tabuleiro e atualiza o estado atual.
        ///     Não verifica se o movimento é legal (xeque). Use a função "MovimentoFeitoLegal".
        /// </summary>
        /// <see cref="MovimentoFeitoLegal(Movimento)" />
        /// <remarks>
        ///     Esta é uma função importante, uma vez que tem que garantir que o estado atual seja atualizado corretamente.
        ///     Também precisa ter um bom desempenho porque vai ser executada milhões de vezes durante a pesquisa.
        ///     Uma melhoria que pode ser feita é atualizar a chave zobrista incremental. Atualmente, nós calculamos isso
        ///     para todo o tabuleiro de xadrez, mas pode ser atualizado com apenas as informações que mudaram.
        ///     Isso deve melhorar o desempenho um pouco.
        ///     Por exemplo: basta atualizar os dados dos quadrados afetados pelo movimento.
        /// </remarks>
        /// <param name="movimento">movimento a ser executado</param>
        public void FazMovimento(Movimento movimento)
        {
            Historia[IndiceHistoria].Salva(this, movimento);

            IndiceEnPassant = 0;

            MovePeca(movimento.IndiceOrigem, movimento.IndiceDestino);
            if (movimento.Promocao()) ColocaPeca(movimento.IndiceDestino, movimento.PecaPromocao);
            if (movimento.IndicePeaoEnPassant != 0) Quadrados[movimento.IndicePeaoEnPassant] = Peca.Nenhuma;
            if (movimento.Peca == Peca.PeaoBranco && Defs.PrimeiraFileiraPeaoBranco(movimento.IndiceOrigem))
                if (movimento.IndiceDestino == movimento.IndiceOrigem + Defs.POSICAO_NORTE * 2)
                    IndiceEnPassant = movimento.IndiceOrigem + Defs.POSICAO_NORTE;
            if (movimento.Peca == Peca.PeaoPreto && Defs.PrimeiraFileiraPeaoPreto(movimento.IndiceOrigem))
                if (movimento.IndiceDestino == movimento.IndiceOrigem + Defs.POSICAO_SUL * 2)
                    IndiceEnPassant = movimento.IndiceOrigem + Defs.POSICAO_SUL;

            if (movimento.RoqueE1G1()) MovePeca((int) Defs.INDICE.H1, (int) Defs.INDICE.F1);
            if (movimento.RoqueE1C1()) MovePeca((int) Defs.INDICE.A1, (int) Defs.INDICE.D1);
            if (movimento.RoqueE8G8()) MovePeca((int) Defs.INDICE.H8, (int) Defs.INDICE.F8);
            if (movimento.RoqueE8C8()) MovePeca((int) Defs.INDICE.A8, (int) Defs.INDICE.D8);

            if (movimento.Peca == Peca.ReiBranco) RoqueE1C1 = RoqueE1G1 = false;
            if (movimento.Peca == Peca.TorreBranca)
            {
                if (movimento.IndiceOrigem == (int) Defs.INDICE.H1) RoqueE1G1 = false;
                if (movimento.IndiceOrigem == (int) Defs.INDICE.A1) RoqueE1C1 = false;
            }

            if (movimento.Peca == Peca.ReiPreto) RoqueE8C8 = RoqueE8G8 = false;
            if (movimento.Peca == Peca.TorrePreta)
            {
                if (movimento.IndiceOrigem == (int) Defs.INDICE.H8) RoqueE8G8 = false;
                if (movimento.IndiceOrigem == (int) Defs.INDICE.A8) RoqueE8C8 = false;
            }

            if (CorJogar == Cor.Preta) ContadorMovimentos++;

            if (movimento.Peca == Peca.PeaoBranco || movimento.Peca == Peca.PeaoPreto || movimento.Captura())
                ContadorRegra50 = 0;
            else
                ContadorRegra50++;

            CorJogar = CorJogar.Invertida();

            Chave = Zobrist.ObtemChave(this);

            IndiceHistoria++;
        }

        /// <summary>
        ///     Desfaz o movimento feito anteriormente. Obtém o movimento anterior da tabela de histórico.
        /// </summary>
        /// <remarks>
        ///     A maioria das informações pode ser restaurada a partir da tabela de histórico.
        /// </remarks>
        public void DesfazMovimento()
        {
            if (IndiceHistoria == 0) return;

            IndiceHistoria--;
            Historia[IndiceHistoria].Restaura(this);
            var movimento = Historia[IndiceHistoria].Movimento;

            if (movimento.Promocao())
            {
                ColocaPeca(movimento.IndiceOrigem, movimento.Peca);
                AtribuiCasaVazia(movimento.IndiceDestino);
            }
            else
            {
                MovePeca(movimento.IndiceDestino, movimento.IndiceOrigem);
            }

            if (movimento.Captura()) ColocaPeca(movimento.IndiceDestino, movimento.PecaCaptura);
            if (movimento.IndicePeaoEnPassant != 0)
                Quadrados[movimento.IndicePeaoEnPassant] = movimento.Peca.InverteCor();
            if (movimento.RoqueE1G1()) MovePeca((int) Defs.INDICE.F1, (int) Defs.INDICE.H1);
            if (movimento.RoqueE1C1()) MovePeca((int) Defs.INDICE.D1, (int) Defs.INDICE.A1);
            if (movimento.RoqueE8G8()) MovePeca((int) Defs.INDICE.F8, (int) Defs.INDICE.H8);
            if (movimento.RoqueE8C8()) MovePeca((int) Defs.INDICE.D8, (int) Defs.INDICE.A8);

            if (CorJogar == Cor.Branca) ContadorMovimentos--;

            CorJogar = CorJogar.Invertida();
        }

        /// <summary>
        ///     Faz o movimento nulo.
        /// </summary>
        /// <remarks>
        ///     O movimento nulo é uma técnica usada pela pesquisa. Basicamente, daremos o
        ///     direito de fazer um movimento para o adversário e atualizaremos todas as informações.
        ///     A única coisa que não vai mudar será o tabuleiro de xadrez.
        /// </remarks>
        /// <see cref="Pesquisa" />
        public void FazMovimentoNulo()
        {
            Historia[IndiceHistoria].Salva(this, null);
            IndiceEnPassant = 0;
            if (CorJogar == Cor.Preta) ContadorMovimentos++;
            ContadorRegra50++;
            CorJogar = CorJogar.Invertida();
            Chave = Zobrist.ObtemChave(this);
            IndiceHistoria++;
        }

        /// <summary>
        ///     Desfaz o movimento nulo.
        /// </summary>
        /// <see cref="Pesquisa" />
        /// <see cref="FazMovimentoNulo" />
        public void DesfazMovimentoNulo()
        {
            if (IndiceHistoria == 0) return;
            IndiceHistoria--;
            Historia[IndiceHistoria].Restaura(this);
            if (CorJogar == Cor.Branca) ContadorMovimentos--;
            CorJogar = CorJogar.Invertida();
        }

        /// <summary>
        ///     Indica se o movimento anterior foi nulo.
        /// </summary>
        /// <remarks>
        ///     Normalmente, um programa de xadrez evitará dois movimentos nulos em seqüência.
        /// </remarks>
        /// <returns>Verdadeiro quando o movimento anterior foi nulo</returns>
        public bool MovimentoAnteriorFoiNulo()
        {
            if (IndiceHistoria == 0) return false;
            return Historia[IndiceHistoria - 1] == null;
        }

        /// <summary>
        ///     Indica se o tabuleiro de xadrez tem alguma peça: dama, torre, bispo ou cavalo.
        /// </summary>
        /// <remarks>
        ///     Normalmente usado pela técnica de movimento nulo.
        /// </remarks>
        /// <param name="cor">Cor a ser verificada</param>
        /// <returns>Verdadeito se cor possui pelo menos uma das peças: dama, torre, bispo ou cavalo.</returns>
        public bool TemPecas(Cor cor)
        {
            for (var fileira = Defs.PRIMEIRA_FILEIRA; fileira < Defs.ULTIMA_FILEIRA; fileira++)
            for (var coluna = Defs.PRIMEIRA_COLUNA; coluna < Defs.ULTIMA_COLUNA; coluna++)
            {
                var indice = fileira * Defs.NUMERO_COLUNAS + coluna;
                var item = Quadrados[indice];
                if (item == Peca.Nenhuma) continue;
                if (cor == Cor.Branca)
                    return item == Peca.CavaloBranco || item == Peca.BispoBranco || item == Peca.TorreBranca ||
                           item == Peca.DamaBranca;
                return item == Peca.CavaloPreto || item == Peca.BispoPreto || item == Peca.TorrePreta ||
                       item == Peca.DamaPreta;
            }

            return false;
        }

        /// <summary>
        ///     Indica se o movimento feito é legal: o próprio rei não está em xeque e,
        ///     no caso do roque, as casas que o rei "passou" não estão sob ataque.
        /// </summary>
        /// <remarks>
        ///     Esta função verifica a legalidade após o movimento ser feito. Outra possibilidade
        ///     é criar uma função que verifique ANTES de fazer o movimento. É mais complicado, mas
        ///     pode economizar algum tempo, uma vez que não atualiza o estado do tabuleiro de xadrez
        ///     e evita ter que fazer/desfazer o movimento.
        /// </remarks>
        /// <returns>Verdadeiro/falso quando o movimento é ou não</returns>
        public bool MovimentoFeitoLegal()
        {
            if (IndiceHistoria == 0) return false;
            var movimento = Historia[IndiceHistoria - 1].Movimento;

            if (CorJogar == Cor.Branca)
            {
                Debug.Assert(ObtemPeca(IndiceReiPreto) == Peca.ReiPreto);
                if (CasaAtacada(IndiceReiPreto, Cor.Branca)) return false;
                if (movimento.RoqueE8G8())
                {
                    if (CasaAtacada((int) Defs.INDICE.E8, Cor.Branca)) return false;
                    if (CasaAtacada((int) Defs.INDICE.F8, Cor.Branca)) return false;
                }

                if (movimento.RoqueE8C8())
                {
                    if (CasaAtacada((int) Defs.INDICE.E8, Cor.Branca)) return false;
                    if (CasaAtacada((int) Defs.INDICE.D8, Cor.Branca)) return false;
                    if (CasaAtacada((int) Defs.INDICE.C8, Cor.Branca)) return false;
                }
            }
            else
            {
                Debug.Assert(ObtemPeca(IndiceReiBranco) == Peca.ReiBranco);
                if (CasaAtacada(IndiceReiBranco, Cor.Preta)) return false;
                if (movimento.RoqueE1G1())
                {
                    if (CasaAtacada((int) Defs.INDICE.E1, Cor.Preta)) return false;
                    if (CasaAtacada((int) Defs.INDICE.F1, Cor.Preta)) return false;
                }

                if (movimento.RoqueE1C1())
                {
                    if (CasaAtacada((int) Defs.INDICE.E1, Cor.Preta)) return false;
                    if (CasaAtacada((int) Defs.INDICE.D1, Cor.Preta)) return false;
                    if (CasaAtacada((int) Defs.INDICE.C1, Cor.Preta)) return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Indica se, na posição atual, atingimos o empate de repetição.
        /// </summary>
        /// <returns>Verdadeiro se a posição atual se repetiu pelo menos 3 vezes</returns>
        public bool EmpatePorRepeticao()
        {
            var repeticoes = 0;
            for (var i = IndiceHistoria - 2; i >= 0; i -= 2)
                if (Chave == Historia[i].Chave)
                    repeticoes++;
            // pelo menos 2 vezes na história + 1 vez na posição atual = 3 repetições.
            return repeticoes > 1 ? true : false;
        }

        /// <summary>
        ///     Indica se alcançamos 50 movimentos sem capturas ou movimentos de peões.
        /// </summary>
        /// <returns>Verdadeiro se alcançamos a regra do empate dos 50 movimentos.</returns>
        public bool EmpatePorRegra50()
        {
            return ContadorRegra50 >= 100 ? true : false;
        }

        /// <summary>
        ///     Indica se o lado a mover está com seu rei em xeque.
        /// </summary>
        /// <returns>Verdadeiro se o rei está em xeque</returns>
        public bool CorJogarEstaEmCheque()
        {
            if (CorJogar == Cor.Branca)
                return CasaAtacada(IndiceReiBranco, Cor.Preta);
            return CasaAtacada(IndiceReiPreto, Cor.Branca);
        }

        /// <summary>
        ///     Indica se a casa está sendo atacado por peças inimigas.
        /// </summary>
        /// <param name="indice">Índice da casa a ser verificada</param>
        /// <param name="cor_atacante">Cor do lado atacante.</param>
        /// <returns>Verdadeiro se a casa está sob ataque pela cor atacante</returns>
        private bool CasaAtacada(int indice, Cor cor_atacante)
        {
            Debug.Assert(Quadrados[indice] != Peca.Borda);
            Debug.Assert(cor_atacante == Cor.Branca || cor_atacante == Cor.Preta);

            if (CasaAtacadaPorPeao(indice, cor_atacante)) return true;
            if (CasaAtacadaPorCavalo(indice, cor_atacante)) return true;
            if (CasaAtacadaPorTorreDama(indice, cor_atacante)) return true;
            if (CasaAtacadaPorBispoDama(indice, cor_atacante)) return true;
            if (CasaAtacadaPorRei(indice, cor_atacante)) return true;
            return false;
        }

        /// <summary>
        ///     Indica se a casa está sendo atacado por peões inimigos.
        /// </summary>
        /// <param name="indice">Índice da casa a ser verificada</param>
        /// <param name="cor_atacante">Cor do lado atacante.</param>
        /// <returns>Verdadeiro se a casa está sob ataque</returns>
        private bool CasaAtacadaPorPeao(int indice, Cor cor_atacante)
        {
            Debug.Assert(Quadrados[indice] != Peca.Borda);
            Debug.Assert(cor_atacante == Cor.Branca || cor_atacante == Cor.Preta);

            if (cor_atacante == Cor.Branca)
            {
                if (ObtemPeca(indice + Defs.POSICAO_SUDESTE) == Peca.PeaoBranco) return true;
                if (ObtemPeca(indice + Defs.POSICAO_SUDOESTE) == Peca.PeaoBranco) return true;
            }
            else
            {
                if (ObtemPeca(indice + Defs.POSICAO_NORDESTE) == Peca.PeaoPreto) return true;
                if (ObtemPeca(indice + Defs.POSICAO_NOROESTE) == Peca.PeaoPreto) return true;
            }

            return false;
        }

        /// <summary>
        ///     Indica se a casa está sendo atacado por cavalos inimigos.
        /// </summary>
        /// <param name="indice">Índice da casa a ser verificada</param>
        /// <param name="cor_atacante">Cor do lado atacante.</param>
        /// <returns>Verdadeiro se a casa está sob ataque</returns>
        private bool CasaAtacadaPorCavalo(int indice, Cor cor_atacante)
        {
            Debug.Assert(Quadrados[indice] != Peca.Borda);
            Debug.Assert(cor_atacante == Cor.Branca || cor_atacante == Cor.Preta);

            var cavalo = cor_atacante == Cor.Branca ? Peca.CavaloBranco : Peca.CavaloPreto;

            foreach (var movimento in Defs.Movimentos.CAVALO)
                if (ObtemPeca(indice + movimento) == cavalo)
                    return true;

            return false;
        }

        /// <summary>
        ///     Indica se a casa está sendo atacado por torre ou dama inimigos. Linhas horizontais e verticais.
        /// </summary>
        /// <param name="indice">Índice da casa a ser verificada</param>
        /// <param name="cor_atacante">Cor do lado atacante.</param>
        /// <returns>Verdadeiro se a casa está sob ataque pela torre ou dama</returns>
        private bool CasaAtacadaPorTorreDama(int indice, Cor cor_atacante)
        {
            Debug.Assert(Quadrados[indice] != Peca.Borda);
            Debug.Assert(cor_atacante == Cor.Branca || cor_atacante == Cor.Preta);

            foreach (var movimento in Defs.Movimentos.TORRE)
            {
                var indice_destino = indice + movimento;
                while (!BordaDoTabuleiro(indice_destino))
                {
                    var tipoPeca = ObtemPeca(indice_destino).ParaTipo();
                    if (tipoPeca == TipoPeca.Torre || tipoPeca == TipoPeca.Dama) return true;
                    if (tipoPeca != TipoPeca.Nenhum) break;
                    indice_destino += movimento;
                }
            }

            return false;
        }

        /// <summary>
        ///     Indica se a casa está sendo atacado por torre ou dama inimigos. Linhas diagonais.
        /// </summary>
        /// <param name="indice">Índice da casa a ser verificada</param>
        /// <param name="cor_atacante">Cor do lado atacante.</param>
        /// <returns>Verdadeiro se a casa está sob ataque plo bispo ou dama</returns>
        private bool CasaAtacadaPorBispoDama(int indice, Cor cor_atacante)
        {
            Debug.Assert(Quadrados[indice] != Peca.Borda);
            Debug.Assert(cor_atacante == Cor.Branca || cor_atacante == Cor.Preta);

            foreach (var movimento in Defs.Movimentos.BISPO)
            {
                var indice_destino = indice + movimento;
                while (!BordaDoTabuleiro(indice_destino))
                {
                    var tipoPeca = ObtemPeca(indice_destino).ParaTipo();
                    if (tipoPeca == TipoPeca.Bispo || tipoPeca == TipoPeca.Dama) return true;
                    if (tipoPeca != TipoPeca.Nenhum) break;
                    indice_destino += movimento;
                }
            }

            return false;
        }

        /// <summary>
        ///     Indica se a casa está sendo atacado pelo rei inimigo.
        /// </summary>
        /// <param name="indice">Índice da casa a ser verificada</param>
        /// <param name="cor_atacante">Cor do lado atacante.</param>
        /// <returns>Verdadeiro se a casa está sob ataque</returns>
        private bool CasaAtacadaPorRei(int indice, Cor cor_atacante)
        {
            Debug.Assert(Quadrados[indice] != Peca.Borda);
            Debug.Assert(cor_atacante == Cor.Branca || cor_atacante == Cor.Preta);

            foreach (var movimento in Defs.Movimentos.REI)
                if (ObtemPeca(indice + movimento).ParaTipo() == TipoPeca.Rei)
                    return true;

            return false;
        }

        /// <summary>
        ///     Retorna peça atual na casa do tabuleiro.
        /// </summary>
        /// <param name="indice">Índice da casa</param>
        /// <returns></returns>
        public Peca ObtemPeca(int indice)
        {
            Debug.Assert(indice >= 0 && indice < Defs.INDICE_MAXIMO);

            return Quadrados[indice];
        }

        /// <summary>
        ///     Coloca peça na casa do tabuleiro.
        /// </summary>
        /// <param name="indice_destino">Índice da casa</param>
        /// <param name="peca">Peça a ser colocada</param>
        public void ColocaPeca(int indice_destino, Peca peca)
        {
            Debug.Assert(Defs.Converte12x12Para8x8(indice_destino) >= 0 &&
                         Defs.Converte12x12Para8x8(indice_destino) < 64);
            Debug.Assert(Quadrados[indice_destino] != Peca.Borda);
            Debug.Assert(peca != Peca.Nenhuma);

            Quadrados[indice_destino] = peca;
            if (peca == Peca.ReiBranco) IndiceReiBranco = indice_destino;
            if (peca == Peca.ReiPreto) IndiceReiPreto = indice_destino;
        }

        /// <summary>
        ///     Esvazia casa do tabuleiro.
        /// </summary>
        /// <param name="indice">Índice da casa</param>
        private void AtribuiCasaVazia(int indice)
        {
            Debug.Assert(indice >= 0 && indice <= Defs.INDICE_MAXIMO);
            Debug.Assert(Quadrados[indice] != Peca.Borda);

            Quadrados[indice] = Peca.Nenhuma;
        }

        /// <summary>
        ///     Move peça da casa origem para casa destino.
        /// </summary>
        /// <param name="indice_origem">Índice da casa origem</param>
        /// <param name="indice_destino">Índice da casa destino</param>
        private void MovePeca(int indice_origem, int indice_destino)
        {
            Debug.Assert(indice_origem >= 0 && indice_origem < Defs.INDICE_MAXIMO);
            Debug.Assert(indice_destino >= 0 && indice_destino < Defs.INDICE_MAXIMO);
            Debug.Assert(Quadrados[indice_origem] != Peca.Borda);
            Debug.Assert(Quadrados[indice_destino] != Peca.Borda);

            Quadrados[indice_destino] = Quadrados[indice_origem];
            Quadrados[indice_origem] = Peca.Nenhuma;
            if (Quadrados[indice_destino] == Peca.ReiBranco) IndiceReiBranco = indice_destino;
            if (Quadrados[indice_destino] == Peca.ReiPreto) IndiceReiPreto = indice_destino;
        }

        /// <summary>
        ///     Função utilitária que indica a casa tem uma peça branca.
        /// </summary>
        /// <param name="indice">Índice da casa</param>
        /// <returns>Verdadeiro para peça branca</returns>
        public bool CasaComPecaBranca(int indice)
        {
            Debug.Assert(indice >= 0 && indice < Defs.INDICE_MAXIMO);

            return Quadrados[indice].ParaCor().Equals(Cor.Branca);
        }

        /// <summary>
        ///     Função utilitária que indica a casa tem uma peça preta.
        /// </summary>
        /// <param name="indice">Índice da casa</param>
        /// <returns>Verdadeiro para peça preta</returns>
        public bool CasaComPecaPreta(int indice)
        {
            Debug.Assert(indice >= 0 && indice < Defs.INDICE_MAXIMO);

            return Quadrados[indice] >= Peca.ReiPreto && Quadrados[indice] <= Peca.PeaoPreto;
        }

        /// <summary>
        ///     Função utilitária que indica a casa está vazia.
        /// </summary>
        /// <param name="indice">Índice da casa</param>
        /// <returns>Verdadeiro para casa vazia</returns>
        public bool CasaVazia(int indice)
        {
            Debug.Assert(indice >= 0 && indice < Defs.INDICE_MAXIMO);

            return Quadrados[indice] == Peca.Nenhuma;
        }

        /// <summary>
        ///     Função utilitária que indica a casa é a borda do tabuleiro de xadrez
        /// </summary>
        /// <param name="indice">Índice da casa</param>
        /// <returns>Verdadeiro para borda do tabuleiro</returns>
        public bool BordaDoTabuleiro(int indice)
        {
            Debug.Assert(indice >= 0 && indice < Defs.INDICE_MAXIMO);

            return Quadrados[indice] == Peca.Borda;
        }

        /// <summary>
        ///     Representação na forma de string do tabuleiro de xadrez.
        /// </summary>
        /// <remarks>
        ///     Somente para visualizacao durante testes. Normalmente um programa grafico
        ///     vai gerenciar a interface do jogo de xadrez.
        /// </remarks>
        /// <returns>string representado o tabuleiro</returns>
        public override string ToString()
        {
            var s = "";

            for (var fileira = Defs.PRIMEIRA_FILEIRA; fileira < Defs.ULTIMA_FILEIRA; fileira++)
            {
                s += "+---+---+---+---+---+---+---+---+" + Environment.NewLine;
                s += "|";
                for (var coluna = Defs.PRIMEIRA_COLUNA; coluna < Defs.ULTIMA_COLUNA; coluna++)
                {
                    s += " ";
                    s += Quadrados[fileira * Defs.NUMERO_COLUNAS + coluna].ParaTexto();
                    s += " ";
                    s += "|";
                }

                s += Environment.NewLine;
            }

            s += "+---+---+---+---+---+---+---+---+" + Environment.NewLine;
            s += "FEN: " + ObtemFEN();
            s += IndiceEnPassant != 0 ? " EnPassant: " + Defs.COORDENADAS[IndiceEnPassant] : "";
            s += " Regra50: " + ContadorRegra50;
            s += " Movimentos: " + ContadorMovimentos;
            s += Environment.NewLine;

            return s;
        }
    }
}
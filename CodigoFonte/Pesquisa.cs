using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Diagnostics;

namespace Enxadrista
{
    public class Pesquisa
    {
        public Tabuleiro Tabuleiro;
        public Avaliacao Avaliacao;
        public Transposicao Transposicao;
        public Ordenacao Ordenacao = new Ordenacao();

        public bool ImprimeInformacao = true;
        public ulong ContadorPosicoes = 0;
        public bool EncerraProcura = false;
        public int MilisegundosLimite = 0;
        public int ProfundidadeLimite = 0;
        public int ProfundidadeAtual = 0;
        public Movimento MelhorMovimento = null;

        private Stopwatch ControleTempo = new Stopwatch();

        public Pesquisa(Tabuleiro tabuleiro, Avaliacao avaliacao, Transposicao transposicao)
        {
            Transposicao = transposicao;
            Tabuleiro = tabuleiro;
            Avaliacao = avaliacao;
        }

        public void LoopAprofundamentoInterativo(int milisegundo_limite, int profundidade_limite)
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
                //Console.Write("PA: " + ProfundidadeAtual + " GC:" + GC.GetTotalMemory(false));
                //Console.WriteLine(" AFTER:" + GC.GetTotalMemory(false));
                if (EncerraProcura) break;
                if (ControleTempo.ElapsedMilliseconds > (int)(MilisegundosLimite * 0.60)) break;

                GC.Collect();
            }

            ControleTempo.Stop();

            MelhorMovimento = variacao_principal.Count > 0 ? variacao_principal[0] : null;

            //Console.WriteLine(TabelaMelhores.Count);
        }

        public int AlfaBeta(int alfa, int beta, int nivel, int profundidade, List<Movimento> variacao_principal)
        {
            Debug.Assert(alfa >= Defs.VALOR_MINIMO);
            Debug.Assert(beta <= Defs.VALOR_MAXIMO);
            Debug.Assert(beta > alfa);
            Debug.Assert(nivel >= 0 && nivel <= Defs.NIVEL_MAXIMO);
            Debug.Assert(profundidade >= 0 && profundidade <= Defs.PROFUNDIDADE_MAXIMA);
            Debug.Assert(variacao_principal != null);

            if (nivel > 0 && Tabuleiro.EmpatePorRegra50()) return 0;
            if (nivel > 0 && Tabuleiro.EmpatePorRepeticao()) return 0;

            if (profundidade <= 0) return Quiescente(alfa, beta, nivel, variacao_principal);

            VerificaTerminoProcura();
            if (EncerraProcura) return 0;
            ContadorPosicoes++;
            if (nivel > 0) variacao_principal.Clear();

            if (nivel > Defs.NIVEL_MAXIMO - 1) return Avaliacao.ObtemPontuacao();

            Movimento movimento_transposicao = null;

            var registro = Transposicao.Recupera(Tabuleiro.Chave, profundidade);
            if (registro != null) {
                if (registro.PodeUsarValor(alfa, beta)) {
                    return Transposicao.AjustaValorParaProcura(registro.Valor, nivel);
                }
                movimento_transposicao = registro.Movimento;
            }

            var cor_jogar_esta_em_cheque = Tabuleiro.CorJogarEstaEmCheque();
            var valor_avaliacao = Avaliacao.ObtemPontuacao();
            var nova_variacao_principal = new List<Movimento>();

            if (profundidade <= 3 && !cor_jogar_esta_em_cheque && valor_avaliacao + 150 * profundidade < alfa) {
                int alfa_reduzido = alfa - 150 * profundidade;
                int valor = Quiescente(alfa_reduzido, alfa_reduzido + 1, nivel, nova_variacao_principal);
                if (valor <= alfa_reduzido) return valor;
            }

            if (profundidade > 3 && !cor_jogar_esta_em_cheque && alfa == beta - 1 && valor_avaliacao >= beta) { 
                if (!Tabuleiro.MovimentoAnteriorFoiNulo() && Tabuleiro.TemPecas(Tabuleiro.CorJogar)) {
                    Tabuleiro.FazMovimentoNulo();
                    int valor = -AlfaBeta(-beta, -beta + 1, nivel + 1, profundidade - 3, nova_variacao_principal);
                    Tabuleiro.DesfazMovimentoNulo();
                    if (EncerraProcura) return 0;
                    if (valor >= beta) {
                        if (valor > Defs.AVALIACAO_MAXIMA) valor = beta;
                        Transposicao.Salva(Tabuleiro.Chave, profundidade, valor, nivel, Transposicao.REGISTRO_INFERIOR, null);
                        return valor;
                    }
                }
            }

            int melhor_valor = Defs.VALOR_MINIMO;
            int contador_movimentos = 0;
            Movimento melhor_movimento = null;

            int nova_profundidade = profundidade - 1;
            if (cor_jogar_esta_em_cheque) nova_profundidade += 1;

            var lista = Tabuleiro.GeraMovimentos();
            lista = Ordenacao.Orderna(Tabuleiro.CorJogar, lista, movimento_transposicao);

            foreach (var movimento in lista) {
                Tabuleiro.FazMovimento(movimento);
                if (!Tabuleiro.MovimentoFeitoLegal()) {
                    Tabuleiro.DesfazMovimento();
                    continue;
                }
                Debug.Assert(Tabuleiro.Chave == Zobrist.ObtemChave(Tabuleiro));

                contador_movimentos += 1;

                int valor_procura = 0;
                if (melhor_valor == Defs.VALOR_MINIMO) {
                    valor_procura = -AlfaBeta(-beta, -alfa, nivel + 1, nova_profundidade, nova_variacao_principal);
                }
                else {
                    if (!cor_jogar_esta_em_cheque && nova_profundidade == 1 && !movimento.Tatico() && alfa == beta - 1 && valor_avaliacao + 100 < alfa) {
                        Tabuleiro.DesfazMovimento();
                        continue;
                    }
                    int reducao = 0;
                    if (!cor_jogar_esta_em_cheque && nova_profundidade > 1 && contador_movimentos > 4 && !movimento.Tatico() && alfa == beta - 1 && valor_avaliacao < alfa) {
                        reducao = 1;
                    }
                    valor_procura = -AlfaBeta(-alfa - 1, -alfa, nivel + 1, nova_profundidade - reducao, nova_variacao_principal);
                    if (!EncerraProcura && valor_procura > alfa && reducao != 0) {
                        valor_procura = -AlfaBeta(-alfa - 1, -alfa, nivel + 1, nova_profundidade, nova_variacao_principal);
                    }
                    if (!EncerraProcura && valor_procura > alfa && valor_procura < beta) {
                        valor_procura = -AlfaBeta(-beta, -alfa, nivel + 1, nova_profundidade, nova_variacao_principal);
                    }
                }

                Tabuleiro.DesfazMovimento();
                if (EncerraProcura) return 0;
                Debug.Assert(Tabuleiro.Chave == Zobrist.ObtemChave(Tabuleiro));

                if (valor_procura >= beta) {
                    Ordenacao.AtualizaHistoria(Tabuleiro.CorJogar, movimento, profundidade);
                    Transposicao.Salva(Tabuleiro.Chave, profundidade, valor_procura, nivel, Transposicao.REGISTRO_INFERIOR, movimento);
                    return valor_procura;
                }

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

            if (melhor_valor == Defs.VALOR_MINIMO)
                return cor_jogar_esta_em_cheque ? -Defs.VALOR_MATE + nivel : 0;

            if (melhor_movimento != null) {
                Ordenacao.AtualizaHistoria(Tabuleiro.CorJogar, melhor_movimento, profundidade);
                Transposicao.Salva(Tabuleiro.Chave, profundidade, melhor_valor, nivel, Transposicao.REGISTRO_EXATO, melhor_movimento);
            }
            else {
                Transposicao.Salva(Tabuleiro.Chave, profundidade, melhor_valor, nivel, Transposicao.REGISTRO_SUPERIOR, null);
            }

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

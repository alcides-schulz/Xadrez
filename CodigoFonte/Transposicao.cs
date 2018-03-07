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
    /// Enxadrista usa uma tabela muito pequena, mas isso deve dar uma boa idéia de como 
    /// funciona.
    /// A tabela terá várias entradas dependendo da memória disponível. Normalmente, o 
    /// número de entradas é definido quando o programa é iniciado com base no tamanho da tabela.
    /// Cada entrada terá quatro registros, onde podemos armazenar informações sobre a posição.
    /// Cada registro terá informações sobre uma posição diferente.
    /// Esta estrutura pode variar de programa para programa.
    /// A posição é identificada pela chave zobrist, você pode ver como ela é calculada na
    /// classe zobrist.cs.
    /// Vamos armazenar no registro, a profundidade, o valor da posição, o melhor movimento
    /// e um valor para indicar qual o tipo de valor que temos. Veja a classe Registro para
    /// mais detalhes sobre estes dados.
    /// 
    ///             //Motor.Tabuleiro.NovaPartida("8/k/3p4/p2P1p2/P2P1P2/8/8/K7 w - - 0 1");
    ///             //Motor.Tabuleiro.NovaPartida("2k5/8/1pP1K3/1P6/8/8/8/8 w - -");
    /// </remarks>
    /// <see cref="Transposicao.Registro"/>
    /// <see cref="Zobrist"/>
    public class Transposicao
    {
        public class Registro
        {
            public ulong Chave;
            public int Profundidade;
            public int Valor;
            public Movimento Movimento;
            public byte Geracao;
            public byte Tipo;

            public void Inicializa()
            {
                Chave = 0;
                Profundidade = 0;
                Valor = 0;
                Movimento = null;
                Geracao = 0;
                Tipo = 0;
            }

            public bool PodeUsarValor(int alfa, int beta)
            {
                if (Tipo == Transposicao.REGISTRO_SUPERIOR && Valor <= alfa) return true;
                if (Tipo == Transposicao.REGISTRO_INFERIOR && Valor >= beta) return true;
                if (Tipo == Transposicao.REGISTRO_EXATO && Valor <= alfa) return true;
                if (Tipo == Transposicao.REGISTRO_EXATO && Valor >= beta) return true;
                return false;
            }
        }

        public const int NUMERO_GRUPOS = 500000;
        public const int NUMERO_REGISTROS = 4;

        public const byte REGISTRO_SUPERIOR = 1;
        public const byte REGISTRO_INFERIOR = 2;
        public const byte REGISTRO_EXATO = 3;

        public Registro[][] Tabela = new Registro[NUMERO_GRUPOS][];

        private byte Geracao;

        public Transposicao()
        {
            for (int indice_grupo = 0; indice_grupo < NUMERO_GRUPOS; indice_grupo++) {
                Tabela[indice_grupo] = new Registro[NUMERO_REGISTROS];
                for (int indice_registro = 0; indice_registro < NUMERO_REGISTROS; indice_registro++) {
                    Tabela[indice_grupo][indice_registro] = new Registro();
                }
            }
            Inicializa();
        }

        public void Inicializa()
        {
            Geracao = 0;
            for (int indice_grupo = 0; indice_grupo < NUMERO_GRUPOS; indice_grupo++) {
                for (int indice_registro = 0; indice_registro < NUMERO_REGISTROS; indice_registro++) {
                    Tabela[indice_grupo][indice_registro].Inicializa();
                }
            }
        }

        public void IncrementaGeracao()
        {
            Geracao++;
        }

        public Registro Recupera(ulong chave, int profundidade)
        {
            Debug.Assert(profundidade >= 0 && profundidade <= Defs.PROFUNDIDADE_MAXIMA);

            int indice_grupo = (int)(chave % NUMERO_GRUPOS);

            Debug.Assert(indice_grupo >= 0 && indice_grupo < Transposicao.NUMERO_GRUPOS);

            var grupo = Tabela[indice_grupo];

            var registro = grupo.Where(r => r.Chave == chave && r.Profundidade >= profundidade).FirstOrDefault();
            if (registro != null) registro.Geracao = Geracao;
            return registro;
        }

        public void Salva(ulong chave, int profundidade, int valor, int nivel, byte tipo, Movimento melhor)
        {
            Debug.Assert(profundidade >= 0 && profundidade <= Defs.PROFUNDIDADE_MAXIMA);
            Debug.Assert(valor >= Defs.VALOR_MINIMO && valor <= Defs.VALOR_MAXIMO);
            Debug.Assert(nivel >= 0 && nivel < Defs.NIVEL_MAXIMO);
            Debug.Assert(tipo == REGISTRO_INFERIOR || tipo == REGISTRO_EXATO || tipo == REGISTRO_SUPERIOR);

            int indice_grupo = (int)(chave % NUMERO_GRUPOS);
            Debug.Assert(indice_grupo >= 0 && indice_grupo < Transposicao.NUMERO_GRUPOS);

            var grupo = Tabela[indice_grupo];

            var registro = grupo.FirstOrDefault(r => r.Chave == chave);
            if (registro == null) registro = grupo.OrderBy(r => r.Geracao).ThenBy(r => r.Profundidade).First();

            registro.Chave = chave;
            registro.Geracao = Geracao;
            registro.Profundidade = profundidade;
            registro.Tipo = tipo;
            registro.Movimento = melhor != null ? melhor : registro.Movimento;
            registro.Valor = Transposicao.AjustaValorParaTabela(valor, nivel);
        }

        public static int AjustaValorParaTabela(int valor, int nivel)
        {
            if (valor > Defs.AVALIACAO_MAXIMA) return valor + nivel;
            if (valor < Defs.AVALIACAO_MINIMA) return valor - nivel;
            return valor;
        }

        public static int AjustaValorParaProcura(int valor, int nivel)
        {
            if (valor > Defs.AVALIACAO_MAXIMA) return valor - nivel;
            if (valor < Defs.AVALIACAO_MINIMA) return valor + nivel;
            return valor;
        }

    }
}

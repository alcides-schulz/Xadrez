using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enxadrista
{
    /// <summary>
    /// Esse código-fonte contém o método "principal" (main) que é chamado quando o programa é iniciado.
    /// </summary>
    /// <remarks>
    /// Um programa que joga xadrez geralmente é chamado de "motor" ("engine"), ou seja, é a parte
    /// que "pensa". A interação com o usuário é feita através da interface gráfica (GUI). 
    /// Aqui vamos nos concentrar na construção do motor. 
    /// 
    /// A parte de interação com o usuário é feita por programas comerciais, por exemplo www.Chessbase.com,
    /// ou você pode usar programas gratuitos como Arena, Winboard e outros. Em geral eu uso Arena.
    /// Mas é bom testar com diferentes programas.
    /// Você pode instalar o Enxadrista.exe como um novo "engine" em um desses programas gráficos.
    /// Em seguida, você joga contra a Enxadrista, ou pode fazê-lo jogar com outros programas já instalados.
    /// 
    /// Se você executar o Enxadrista.exe, você pode dar comandos diretamente ao motor. A sintaxe dos
    /// comandos dependerá do protocolo que você usa.
    /// Existem dois protocolos que podem ser usados: XBoard e UCI. Os programas gráficos geralmente 
    /// aceitam ambos os protocolos. 
    /// Enxadrista é desenvolvido para usar o Xboard, também conhecido como Winboard. No fonte do protocolo 
    /// "Xboard.cs", você tem mais detalhes.
    /// 
    /// Tabuleiro - Board - Tabuleiro.cs
    /// --------------------------------
    /// Este componente é responsável por representar o tabuleiro de xadrez em uma estrutura para manter o 
    /// estado atual do jogo e executar os movimentos do usuário e todos os movimentos usados durante a pesquisa.
    /// Ele também gera a lista de movimentos para a posição.
    /// Este módulo é o mais longo, mas pode ser o mais fácil de entender. Se você conhece as regras do xadrez,
    /// você poderá seguir a lógica.
    /// 
    /// Pesquisa - Search - Pesquisa.cs
    /// -------------------------------
    /// Executa a busca pelo melhor movimento. Esta busca é baseada na força bruta, mas com várias técnicas 
    /// que evitam olhar para todos os movimentos.
    /// A forma mais comum é a pesquisa alfa/beta com variação principal (alpha/beta principal variation 
    /// search).
    /// Algumas técnicas utilizadas durante a pesquisa, que foram programadas neste programa, são:
    /// - pesquisa de janela zero - zero window search
    /// - movimento nulo - null move
    /// - tabela de transposição - tranposition table
    /// - pesquisa quiescente - quiesce search
    /// - poda de futilidade - futility pruning
    /// - redução de movimento tardio - late move reduction
    /// - e outras.
    /// 
    /// Avaliacao - Evaluation - Avaliacao.cs
    /// -------------------------------------
    /// Avalia e gera um valor para a posição no tabuleiro de xadrez. Esta função é usada pela pesquisa.
    /// Deve levar em consideração vários fatores para indicar qual lado tem uma vantagem. 
    /// Assim, juntamente com a procura, o motor determina o melhor. É um conceito importante, o motor 
    /// não está procurando o melhor movimento em si, mas está procurando o movimento que leva às 
    /// melhores posições. Claro que alguns movimentos são decisivos, mas em muitas posições, o motor 
    /// seleciona os movimentos que têm a mais probabilidade de ganhar.
    /// Alguns termos de avaliação que podem ser usados:
    /// - material - o mais importante
    /// - segurança do rei - importante e requer trabalho para funcionar bem
    /// - estrutura do peão - peões isolados, peões duplos, peões passados, etc.
    /// - mobilidade das peças - movimentos disponíveis 
    /// - etc.
    /// 
    /// Como o motor joga xadrez
    /// ------------------------
    /// Basicamente, o programa deve ter o tabuleiro inicializado em uma determinada posição.
    /// Ao receber o comando para procurar a melhor jogada, começará o processo de procura pelo
    /// melhor movimento, ou seja, pesquisa. 
    /// A pesquisa consiste em gerar todos movimentos para a posição e para cada movimento, gerar e 
    /// executar cada resposta. Este processo é repetido recursivamente para cada movimento e é limitado 
    /// por tempo ou profundidade. 
    /// A profundidade é o número de movimentos que o programa poderá alcançar. Por exemplo a profundidade
    /// 8 indica que o programa ve 8 movimentos adiante (4 movimentos do lado branco, e 4 do lado preto).
    /// O programa irá então avaliar a posição para dar um valor e, em seguida, selecionar os movimentos 
    /// que levam a melhores posições.
    /// Um aspecto importante é que o programa de xadrez sempre considera a melhor resposta do adversário 
    /// durante a pesquisa.
    /// 
    /// Como escrever seu programa de xadrez
    /// ------------------------------------
    /// Você pode começar estudando este código-fonte, juntamente com a documentação fornecida. A partir do momento
    /// em que você conhece os componentes, você pode fazer uma cópia do Enxadrista e fazer alterações.
    /// A Enxadrista usa técnicas simples e sem grande otimização. Sempre que possível, deixarei algumas sugestões 
    /// para melhorias e otimizações. 
    /// Você pode tentar isso até estar pronto para escrever seu próprio programa.
    /// O primeiro passo seria escrever a representação do xadrez e o gerador de movimentos. Se você conhece as 
    /// regras do xadrez, você pode tentar escrever esse componente. Obter um gerador de movimentos livre de erros 
    /// já é um grande desafio.
    /// Recomenda-se que se faça uma mudança de cada vez, use "asserts" se a linguagem escolhida permitir, e não se 
    /// apresse com as mudanças, o progresso pode ser lento.
    /// 
    /// O que faz um bom programa de xadrez
    /// -----------------------------------
    /// Existem vários fatores que determinam a força de um programa:
    /// - Livre de erros: definitivamente o mais importante. Um programa de xadrez é complexo e não deve ter erros 
    ///   em sua lógica. Um programa com erros pode jogar xadrez, mas acabará por falhar e escolher o movimento
    ///   errado. Certamente, todo software deve estar livre de erros, mas como vamos procurar milhões de posições, 
    ///   eventualmente, o erro levará a posições erradas. A analogia é a seguinte: imagine um barco com um pequeno
    ///   furo no casco. Pode levar tempo, mas eventualmente ele vai afundar.
    /// - Velocidade: um programa de xadrez visita milhões de posições por segundo, precisa ser rápido. E o mais 
    ///   importante é que a estrutura do programa seja bem definida, tendo um bom desempenho como um todo. Não adianta 
    ///   otimizar uma pequena função do programa. Mas certifique-se que o programa funciona corretamente antes de tentar 
    ///   melhorar o desempenho.
    ///   Para o seu primeiro programa, tente priorizar esta ordem: correção, clareza e desempenho
    /// - Uso de técnicas conhecidas: existem vários conceitos e técnicas que devem ser aplicadas. 
    ///   Por exemplo: movimento nulo (movimento nulo). Este é um conceito que cada programa deve implementar 
    ///   porque aumenta significativamente a força do programa. 
    ///   No início, tente se concentrar no uso de técnicas conhecidas usadas em outros programas, depois você também 
    ///   pode tentar suas próprias idéias, essa é a melhor parte.
    /// - Disciplina e persistência: é muito difícil aumentar a força de um programa de xadrez, você precisa fazer
    ///   uma mudança de cada vez e testar com milhares de jogos rápidos e medir se a mudança surtiu algum efeito (positivo
    ///   ou negativo), então aceitar ou rejeitar. No início, é fácil medir as melhorias, mas com a passagem do tempo
    ///   essas melhorias são menores e mais dificeis de quantificar. 
    ///   Então você deve estabelecer seu processo de desenvolvimento. O software de controle de código-fonte ajuda muito, 
    ///   tipo "git/github".
    /// 
    /// Onde obter mais informações
    /// ---------------------------
    /// - Chess Programming Wiki(https://chessprogramming.wikispaces.com/) - tem tudo sobre programação de programas de xadrez.
    /// - Fórum de programação de xadrez: www.talkchess.com é um dos mais conhecidos.
    /// - Fontes do programa - vários autores fornecem o código-fonte de seu programas. Procure na web, sempre usando o nome do 
    ///   programa e "chess engine". Exemplo "stockfish chess engine"
    ///     - stockfish: este é um dos programas mais fortes do mundo, com a vantagem de ser grátis e ter código aberto.
    ///     - crafty: um dos mais antigos e mais tradicionais e contém uma documentação detalhada das técnicas.
    ///     - fruit: bom programa, pouca documentação, mas mostra elegantemente a implementação de várias técnicas.
    ///     - tcsp: programa de ensino, consulta obrigatória para todos os iniciantes.
    ///     - tucano: meu programa em inglês que me incentivou a escrever esta documentação.
    ///     - Outros programas: existem muitos programas disponíveis. Há uma lista no site wiki de programação de xadrez.
    /// - Técnicas de programação - vários autores publicam artigos relacionados à programação de xadrez.
    /// - Sites de rating: contém o ranking comparativo dos programas. Procure por "chess engine rating"
    /// 
    /// Perguntas e respostas
    /// ---------------------
    /// PERGUNTA: Mas se o stockfish, um dos programas mais fortes do mundo, com seu código fonte disponível porque 
    /// vou escrever meu próprio programa?
    /// RESPOSTA: Você escolhe a sua motivação, pode ser o desafio de aprender algo relacionado a programação. 
    /// No meu caso foi a combinação do interesse ao xadrez e programação.
    /// Você pode fazer algo diferente, como um programa de xadrez em uma linguagem de programação 
    /// diferente, geralmente os programas de xadrez usam C ou C ++.
    /// Ou você pode criar um programa de xadrez que tenha um estilo muito agressivo, que prefere sacrificar peças 
    /// para atacar o rei.
    /// Outra idéia é escrever um programa que simula um jogador de xadrez fraco. Os motores de xadrez são muito 
    /// fortes para os humanos hoje em dia, e é difícil fazer um programa de xadrez simular erros humanos, você pode 
    /// tentar criar um estilo "capivara".
    /// Tudo isso e podemos falar sobre aprendizado de máquina (machine learning), inteligência artificial, que
    /// são um assunto de pesquisa bem interessante (procure no google sobre AlphaZero, que também é pesquisa do google) 
    /// 
    /// PERGUNTA: Por que você não usa idéias do stockfish e outros programas e implementa no seu programa?
    /// RESPOSTA: veja o item "Disciplina e Persistência" acima. As idéias de um programa nem sempre funcionam diretamente 
    /// em outro programa, a maior parte do tempo requer alguma persistência para fazer funcionar. Você pode usar 
    /// idéias como inspiração e se adaptar ao seu programa. Em geral, algo pode funcionar bem em um programa mas não em outro.
    /// 
    /// PERGUNTA: Por que foi escrito na linguagem de programação C#?
    /// RESPOSTA: Eu escolhi C# porque é simples implementar algumas estruturas para que possamos realmente 
    /// nos concentrar nos itens de programação de xadrez.
    /// o maior desafio foi o gerenciamento de memória. Parece que o coletor de lixo foi bem afetado pelo tamanho 
    /// da tabela de transposição. O uso de 64 MB tornou o programa mais lento. Tive que usar uma tabela bem pequena.
    /// Uma vez que o motor de xadrez olhará para milhões de posições, devemos evitar a criação de novos objetos
    /// durante a pesquisa. O uso do comando GC.Collect() ajudou um pouco. Pretendo rever isso no futuro.
    /// Mas o objetivo aqui foi mostrar como usar e explicar as técnicas. Provavelmente, um bom exercício 
    /// seria substituir a criação de objetos (new) e a lista dinâmica por matrizes alocadas no início.
    /// Se você for escrever um programa mais competitivo, provavelmente usará C ou C++.
    /// 
    /// PERGUNTA: Onde posso ver meu programa de xadrez jogar ?
    /// RESPOSTA: Existem muitos torneios online, o principal é TCEC (http://tcec.chessdom.com/live.php). Mas muitos 
    /// entusiastas de xadrez de computador, fazem seus torneios e publicam rankings online e resultados. Um bom ponto
    /// de partida é o fórum www.talkchess.com, há um fórum de torneios lá.
    /// Geralmente, quando você divulga uma nova versão do seu programa, muitas pessoas estarão interessadas em incluir
    /// em seus torneios.
    /// 
    /// PERGUNTA: Por onde eu começo ?
    /// RESPOSTA: Provavelmente, você pode começar pelo módulo Tabuleiro.cs. É feito para representar um tabuleiro
    /// de xadrez e sua geração de movimento. Basta pensar nas regras do xadrez, e você pode seguir a lógica lá.
    /// 
    /// Boa sorte, espero que você também se divirta com programação de xadrez de computador !
    /// 
    /// </remarks>
    public class Programa
    {
        /// <summary>
        /// Começo do programa.
        /// </summary>
        /// <remarks>
        /// Inicializa os componentes do motor e inicia o loop principal da interface.
        /// </remarks>
        /// <param name="args">Nenhum parâmetro é usado neste momento.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Enxadrista 1.0 - Programa jogador de xadrez - Autor: Alcides Schulz");

            var transposicao = new Transposicao();
            var motor = new Motor(transposicao);
            var xboard = new XBoard(motor);

            xboard.LoopPrincipal();
        }
    }
}

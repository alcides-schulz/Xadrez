Enxadrista - Motor de Xadrez
----------------------------

Alcides Schulz - 2018

Enxadrista é um programa que joga xadrez. Este tipo de programa
é conhecido como "engine", ou seja, motor. Existem vários motores de xadrez
mas nem sempre têm uma boa documentação e geralmente está em inglês.

O objetivo aqui é mostrar um programa totalmente funcional, mas incluir
também toda a documentação e dicas sobre como você pode desenvolver o seu
próprio programa. Tudo isso em português!

Eu construí um motor de xadrez antes, é chamado Tucano. Durante esta programação, 
notei que quase todas as informações estão em inglês. Então eu decidi escrever
esta documentação sob a forma de um programa real com comentários em português. 
Espero que ajude outros programadores do Brasil a construir seu motor de xadrez.

Começe com o arquivo Programa.cs que contém informações gerais.
O Enxadrista não é muito forte quando joga contra outros programas, mesmo os mais 
fracos, mas pode ser um bom desafio para um jogador de xadrez amador.
Eu pretendo melhorar isso no futuro.

Enxadrista já foi testado e tem um elo estimado de 1250 em 18/03/2018. O que é baixo 
para os programas de xadrez. Obrigado a Carlos Canavessi que testou contra outros motores. 
Ele organiza torneios online que podem ser seguidos em http://twitch.tv/ccls.
Veja os resultados dos primeiros testes:

    -----------------Enxadrista 1.0 x32----------------- 
    Enxadrista 1.0 x32 - BRAMA 05/12/2004 x32             : 1,0/4 0-2-2 (=0=0)  25%  -191 
    Enxadrista 1.0 x32 - Dragontooth 0.2 Bahamut x64      : 4,0/4 4-0-0 (1111) 100% +1200 
    Enxadrista 1.0 x32 - Iota 1.0 x32                     : 3,5/4 3-0-1 (11=1)  88%  +346 
    Enxadrista 1.0 x32 - Joanna2002 1.06 x32              : 0,5/4 0-3-1 (=000)  13%  -330 
    Enxadrista 1.0 x32 - Nanook v0.17 x32                 : 1,5/4 0-1-3 (=0==)  38%   -85 
    Enxadrista 1.0 x32 - Pierre v1.7 x32                  : 3,0/4 3-1-0 (1110)  75%  +191 
    Enxadrista 1.0 x32 - Project Invincible 2.10 x32      : 1,5/4 1-2-1 (0=10)  38%   -85 
    Enxadrista 1.0 x32 - QuteChess 1.0.1t x32             : 1,5/4 0-1-3 (===0)  38%   -85 
    Enxadrista 1.0 x32 - Sabrina 3.1.25 x64               : 0,5/4 0-3-1 (0=00)  13%  -330 
    Enxadrista 1.0 x32 - Supra 26.0 Pro x64               : 1,0/4 1-3-0 (0100)  25%  -191 
    Enxadrista 1.0 x32 - T.rex 1.8.5 x32                  : 1,0/4 1-3-0 (0100)  25%  -191 
    Enxadrista 1.0 x32 - T.rex 1.9 beta x32               : 2,0/4 2-2-0 (1010)  50%    ±0 
    Enxadrista 1.0 x32 - Talvmenni 0.1 x32                : 1,5/4 1-2-1 (01=0)  38%   -85 
    Enxadrista 1.0 x32 - Toledo Nanochess Jan/11/2010 x32 : 2,5/4 2-1-1 (110=)  63%   +92 
    Enxadrista 1.0 x32 - Usurpator II x32                 : 2,5/4 2-1-1 (=011)  63%   +92 
    Enxadrista 1.0 x32 - Virutor Chess 1.1.2 x32          : 2,0/4 2-2-0 (1010)  50%    ±0

Espero que isso possa ser útil e, em caso de dúvida ou sugestão, envie um e-mail.
Vou responder o melhor que puder.

Um abraço.

Alcides Schulz - autor do programa Tucano.
email: alcides_schulz@hotmail.com
site: https://sites.google.com/site/tucanochess/

Raoni Campos - Autor do programa Pirarucu.
site: https://www.github.com/ratosh/

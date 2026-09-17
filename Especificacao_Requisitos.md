# Especificação de Requisitos de Software (ERS)
## SafeZone-VR — Simulador de Treinamento em Emergências em Realidade Virtual

| Campo | Valor |
|---|---|
| Projeto | SafeZone-VR |
| Documento | Especificação de Requisitos de Software |
| Versão | 1.0 (rascunho) |
| Data | 10/09/2026 |
| Autora | Dai Oliveira |
| Repositório | https://github.com/DaiiOliveira/SafeZone-VR |
| Norma de referência | ISO/IEC/IEEE 29148:2018 (estrutura adaptada da IEEE 830) |

> **Nota sobre premissas:** este documento foi elaborado a partir do escopo informado (fases de alagamento, granizo e incêndio). Os itens marcados com **[CONFIRMAR]** representam decisões técnicas assumidas por padrão e devem ser validadas ou substituídas conforme a implementação real.

---

## 1. Introdução

### 1.1 Propósito

Este documento especifica os requisitos funcionais e não funcionais do **SafeZone-VR**, um simulador em realidade virtual voltado ao treinamento de procedimentos de segurança em três cenários de emergência: alagamento, tempestade de granizo e incêndio.

O público-alvo do documento são: a autora do trabalho, o orientador, avaliadores da banca e eventuais desenvolvedores que venham a manter ou estender o sistema.

### 1.2 Escopo do produto

O SafeZone-VR é uma aplicação de realidade virtual imersiva na qual o usuário é colocado em ambientes tridimensionais sob situação de emergência e deve executar as ações corretas de autoproteção, abrigo e evacuação. O sistema avalia as decisões tomadas e apresenta um relatório de desempenho ao final de cada fase.

**Está no escopo:**

- Três fases jogáveis independentes: alagamento, granizo e incêndio;
- Interação em VR com objetos do cenário (portas, janelas, disjuntores, extintores, objetos de proteção);
- Sistema de avaliação de desempenho por fase, com pontuação e feedback educativo;
- Menu principal com seleção de fase, tutorial e configurações de conforto;
- Registro local do progresso e das pontuações do usuário.

**Não está no escopo (nesta versão):**

- Modo multiusuário ou cooperativo;
- Backend remoto, contas de usuário em nuvem ou ranking online;
- Versão para desktop sem VR (modo *flat screen*);
- Fases adicionais (deslizamento de terra, descargas atmosféricas, acidente químico), previstas apenas como evolução futura;
- Integração com sistemas institucionais de treinamento (LMS/SCORM).

### 1.3 Objetivos do sistema

| ID | Objetivo |
|---|---|
| OBJ01 | Ensinar, por meio de vivência prática simulada, os procedimentos corretos diante de alagamento, tempestade de granizo e incêndio. |
| OBJ02 | Permitir a prática de situações de risco sem exposição física real do treinando. |
| OBJ03 | Fornecer retorno objetivo sobre os acertos e erros cometidos durante a simulação. |
| OBJ04 | Corrigir concepções equivocadas frequentes (por exemplo, retorno ao interior de imóvel em chamas, travessia de área alagada ou permanência sob árvores durante tempestade de granizo). |

### 1.4 Definições, acrônimos e abreviações

| Termo | Definição |
|---|---|
| **VR / RV** | *Virtual Reality* — Realidade Virtual. |
| **HMD** | *Head-Mounted Display* — visor de realidade virtual acoplado à cabeça. |
| **6DoF** | Seis graus de liberdade: rastreamento de posição e rotação da cabeça e das mãos. |
| **Fase** | Cenário jogável completo, correspondente a um tipo de emergência. |
| **Evento crítico** | Ação obrigatória cuja execução (ou omissão) determina o resultado da fase. |
| **Ponto de encontro** | Local seguro predefinido onde o usuário deve chegar para concluir a evacuação. |
| **Granizo** | Precipitação sólida em forma de pedras de gelo, associada a tempestades severas, frequentemente acompanhada de rajadas de vento e descargas atmosféricas. |
| **Abrigo seguro** | Ambiente interno de alvenaria, afastado de aberturas envidraçadas e de coberturas frágeis, onde o usuário deve permanecer durante a tempestade. |
| ***Cybersickness*** | Mal-estar (náusea, tontura) induzido pelo uso de RV. |
| **Teleporte** | Modo de locomoção por saltos instantâneos, com menor indução de *cybersickness*. |
| **Locomoção contínua** | Deslocamento suave por analógico, mais imersivo e mais propenso a *cybersickness*. |
| **PASS** | Mnemônico para uso de extintor: Puxar o pino, Apontar para a base, Apertar o gatilho, Sacudir/varrer a base do fogo. |
| **RF / RNF / RN** | Requisito Funcional / Requisito Não Funcional / Regra de Negócio. |

### 1.5 Referências

- ISO/IEC/IEEE 29148:2018 — *Systems and software engineering — Life cycle processes — Requirements engineering*.
- ABNT NBR 9077 — Saídas de emergência em edifícios (referência conceitual para rotas de fuga, adaptada ao contexto residencial).
- ABNT NBR 12693 — Sistemas de proteção por extintores de incêndio.
- Manuais de orientação da Defesa Civil e do Corpo de Bombeiros Militar quanto a enchentes, tempestades severas e incêndios estruturais.
- Sistema de alertas meteorológicos do INMET e da Defesa Civil de Santa Catarina, quanto a avisos de tempestade com granizo.
- Documentação do Unity XR Interaction Toolkit e do padrão OpenXR. **[CONFIRMAR]**

---

## 2. Descrição geral

### 2.1 Perspectiva do produto

O SafeZone-VR é um produto autônomo (*standalone*), sem dependência de serviços externos em tempo de execução. A arquitetura assumida é: **[CONFIRMAR]**

- **Motor:** Unity, com XR Interaction Toolkit sobre OpenXR;
- **Plataforma-alvo:** HMD autônomo com controles 6DoF (Meta Quest 2/3) e, opcionalmente, execução via PC VR;
- **Persistência:** armazenamento local no dispositivo (arquivo JSON ou `PlayerPrefs`);
- **Distribuição:** instalação direta do pacote de aplicação no dispositivo.

### 2.2 Funções principais do produto

1. Apresentação de menu e seleção de fase;
2. Tutorial de familiarização com os controles;
3. Execução das três fases de emergência;
4. Detecção e avaliação das ações do usuário;
5. Geração de relatório de desempenho ao final de cada fase;
6. Configuração de parâmetros de conforto e acessibilidade;
7. Persistência do progresso e do histórico de tentativas.

### 2.3 Características dos usuários

| Perfil | Descrição | Implicação para os requisitos |
|---|---|---|
| **Treinando iniciante em VR** | Usuário sem experiência prévia com HMD; pode sentir desconforto e ter dificuldade com os controles. | Exige tutorial obrigatório na primeira execução, teleporte como padrão e vinheta de conforto ativa. |
| **Treinando experiente** | Usuário familiarizado com VR, busca repetir fases para melhorar a pontuação. | Exige possibilidade de pular o tutorial, locomoção contínua opcional e histórico de tentativas. |
| **Instrutor / avaliador** | Docente ou responsável por segurança que acompanha a sessão e consulta os resultados. | Exige relatório de desempenho legível e espelhamento da visão do usuário em tela externa. |

### 2.4 Restrições

| ID | Restrição |
|---|---|
| RES01 | O sistema deve executar em hardware autônomo de VR, cujo orçamento gráfico é limitado — o que restringe densidade de polígonos, resolução de texturas e uso de iluminação dinâmica. |
| RES02 | A taxa de quadros não pode cair abaixo do mínimo especificado em RNF01, sob risco de indução de *cybersickness*. |
| RES03 | Toda a interação deve ser possível apenas com dois controles 6DoF, sem teclado ou mouse. |
| RES04 | O conteúdo procedimental das fases deve estar aderente às orientações oficiais brasileiras de Defesa Civil e Corpo de Bombeiros. |
| RES05 | A sessão de uso contínuo não deve exceder a duração recomendada por fase (RNF07), por conforto e higiene do equipamento. |
| RES06 | O sistema deve funcionar integralmente offline. |
| RES07 | O cenário-base é uma residência unifamiliar; requisitos que pressupõem edificação coletiva (elevador, escadas de emergência, brigada, alarme predial) devem ser adaptados a equivalentes domésticos. |

### 2.5 Suposições e dependências

| ID | Suposição |
|---|---|
| SUP01 | O usuário dispõe de área física livre mínima compatível com o *room-scale* configurado no dispositivo, ou joga sentado/em pé sem deslocamento real. |
| SUP02 | O dispositivo já possui limites de segurança (*guardian/boundary*) configurados pelo sistema operacional do HMD. |
| SUP03 | As três fases se passam em uma residência unifamiliar comum, com dois pavimentos, quintal, garagem e telhado em telhas cerâmicas. O mesmo cenário-base é reaproveitado pelas três fases, variando apenas o evento simulado, os danos e os objetos interativos relevantes. |
| SUP04 | Há sempre um acompanhante presente durante a sessão para auxiliar em caso de desconforto do usuário. |

---

## 3. Requisitos funcionais

Prioridade: **Essencial** (indispensável à entrega), **Importante** (agrega valor significativo), **Desejável** (pode ser postergado).

### 3.1 Módulo — Menu e navegação

| ID | Requisito | Prioridade |
|---|---|---|
| RF001 | O sistema deve exibir um menu principal em espaço tridimensional, operável por apontamento com os controles, contendo as opções: Iniciar, Tutorial, Configurações, Histórico e Sair. | Essencial |
| RF002 | O sistema deve permitir a seleção individual de uma das três fases (alagamento, granizo, incêndio), exibindo para cada uma o título, uma breve descrição e a melhor pontuação já obtida. | Essencial |
| RF003 | O sistema deve permitir que o usuário interrompa a fase em andamento a qualquer momento e retorne ao menu principal, descartando a tentativa. | Essencial |
| RF004 | O sistema deve permitir reiniciar a fase corrente sem retornar ao menu principal. | Importante |
| RF005 | O sistema deve exibir o histórico das tentativas anteriores do usuário, com data, fase, pontuação e tempo de conclusão. | Desejável |

### 3.2 Módulo — Tutorial e conforto

| ID | Requisito | Prioridade |
|---|---|---|
| RF006 | O sistema deve apresentar um tutorial de familiarização, executado obrigatoriamente na primeira utilização, que ensine: locomoção, rotação da visão, agarrar objetos e acionar o menu. | Essencial |
| RF007 | O sistema deve permitir que o usuário pule ou repita o tutorial a partir do menu principal. | Importante |
| RF008 | O sistema deve permitir a escolha entre locomoção por teleporte e locomoção contínua, adotando o teleporte como padrão. | Essencial |
| RF009 | O sistema deve permitir a escolha entre rotação por incrementos (*snap turn*) e rotação suave, adotando a rotação por incrementos como padrão. | Essencial |
| RF010 | O sistema deve permitir ativar ou desativar a vinheta de conforto (escurecimento periférico durante o deslocamento), mantendo-a ativa por padrão. | Essencial |
| RF011 | O sistema deve permitir o ajuste da altura do ponto de vista, para acomodar uso sentado ou em pé e diferentes estaturas. | Importante |
| RF012 | O sistema deve permitir a definição da mão dominante, espelhando as funções dos controles. | Desejável |
| RF013 | O sistema deve permitir o ajuste independente do volume dos efeitos sonoros, da narração e da trilha ambiente. | Importante |

### 3.3 Módulo — Interação comum às fases

| ID | Requisito | Prioridade |
|---|---|---|
| RF014 | O sistema deve permitir ao usuário agarrar, soltar e arremessar objetos interativos do cenário com qualquer uma das mãos. | Essencial |
| RF015 | O sistema deve destacar visualmente os objetos interativos quando a mão do usuário estiver a distância de alcance. | Essencial |
| RF016 | O sistema deve permitir abrir e fechar portas por interação física com a maçaneta. | Essencial |
| RF017 | O sistema deve permitir acionar interruptores, disjuntores e alarmes por toque ou pressão do gatilho. | Essencial |
| RF018 | O sistema deve exibir orientações contextuais discretas (texto flutuante ou narração) quando o usuário permanecer inativo por período superior ao limite definido em RN06. | Importante |
| RF019 | O sistema deve emitir realimentação tátil (vibração do controle) ao agarrar objetos, ao colidir com obstáculos e ao executar ações críticas. | Importante |
| RF020 | O sistema deve impedir que o usuário atravesse paredes e objetos sólidos, aplicando bloqueio de deslocamento ou escurecimento da visão. | Essencial |
| RF021 | O sistema deve exibir, durante toda a fase, um indicador de tempo decorrido e o objetivo corrente. | Importante |

### 3.4 Fase 1 — Alagamento

**Contexto:** residência atingida por enchente, com nível de água em elevação progressiva a partir do pavimento térreo. O pavimento superior é o refúgio seguro do cenário.

| ID | Requisito | Prioridade |
|---|---|---|
| RF022 | O sistema deve simular a elevação progressiva do nível da água ao longo da fase, com taxa de subida configurável por parâmetro. | Essencial |
| RF023 | O sistema deve reduzir a velocidade de deslocamento do usuário proporcionalmente à profundidade da água em que ele se encontra. | Essencial |
| RF024 | O sistema deve exigir, como evento crítico, o desligamento do quadro de disjuntores antes que a água atinja tomadas e equipamentos elétricos. | Essencial |
| RF025 | O sistema deve registrar erro grave e encerrar a fase em condição de falha caso o usuário toque em equipamento elétrico energizado estando em contato com a água. | Essencial |
| RF026 | O sistema deve exigir, como evento crítico, a coleta de itens essenciais do kit de emergência (documentos, água potável, lanterna, medicamentos) antes da evacuação. | Essencial |
| RF027 | O sistema deve exigir que o usuário desloque objetos de valor e eletroeletrônicos para pontos elevados, atribuindo pontuação parcial por item preservado. | Importante |
| RF028 | O sistema deve exigir o fechamento do registro de gás antes da evacuação. | Importante |
| RF029 | O sistema deve simular trecho externo com correnteza e registrar erro grave caso o usuário tente atravessá-lo a pé ou de veículo. | Essencial |
| RF030 | O sistema deve concluir a fase com sucesso quando o usuário alcançar o pavimento superior ou o ponto elevado seguro definido no cenário. | Essencial |
| RF031 | O sistema deve simular riscos secundários da enchente, tais como animais peçonhentos deslocados pela água e bueiros abertos submersos, sinalizando-os como zonas de risco. | Desejável |

### 3.5 Fase 2 — Granizo

**Contexto:** o usuário inicia a fase no quintal da residência sob alerta meteorológico iminente, dispõe de uma janela de preparação antes do início da precipitação e deve abrigar-se no interior da casa antes que as pedras de gelo atinjam intensidade perigosa.

| ID | Requisito | Prioridade |
|---|---|---|
| RF032 | O sistema deve apresentar, no início da fase, sinais de aproximação da tempestade: escurecimento do céu, queda de temperatura indicada no ambiente, rajadas de vento e alerta sonoro da Defesa Civil no celular ou rádio do cenário. | Essencial |
| RF033 | O sistema deve dispor de uma janela de preparação, anterior ao início da precipitação, cuja duração é parametrizável, durante a qual o usuário executa as ações preventivas em área externa. | Essencial |
| RF034 | O sistema deve simular a precipitação de granizo com intensidade crescente, com impacto visual e sonoro sobre telhados, vidros, veículos e solo, e com acúmulo progressivo de pedras de gelo no terreno. | Essencial |
| RF035 | O sistema deve exigir, como evento crítico, que o usuário se abrigue em ambiente interno de alvenaria antes do término da janela de preparação. | Essencial |
| RF036 | O sistema deve reduzir progressivamente a integridade do usuário enquanto este permanecer em área desabrigada durante a precipitação, em ritmo proporcional à intensidade corrente do granizo. | Essencial |
| RF037 | O sistema deve registrar erro grave caso o usuário se abrigue sob árvore, poste, estrutura metálica, marquise, telha translúcida ou cobertura de material frágil. | Essencial |
| RF038 | O sistema deve exigir, como ação preventiva, o recolhimento de animais domésticos que estejam em área externa. | Essencial |
| RF039 | O sistema deve exigir, como ação preventiva, o recolhimento ou a fixação de objetos soltos no quintal (vasos, cadeiras, varais, toldos) passíveis de serem arremessados pelo vento. | Importante |
| RF040 | O sistema deve permitir o abrigo do veículo em local coberto durante a janela de preparação, atribuindo pontuação parcial. | Importante |
| RF041 | O sistema deve exigir o fechamento de janelas, portas e persianas antes do início da precipitação, atribuindo pontuação parcial por abertura protegida. | Essencial |
| RF042 | O sistema deve simular o estilhaçamento de vidros e de telhas translúcidas atingidos pelo granizo, e registrar erro grave caso o usuário permaneça próximo a essas aberturas durante a precipitação. | Essencial |
| RF043 | O sistema deve exigir que o usuário se desloque para cômodo interno, afastado de aberturas envidraçadas, durante o pico da tempestade. | Essencial |
| RF044 | O sistema deve simular descargas atmosféricas associadas à tempestade e exigir que o usuário desligue da tomada aparelhos eletroeletrônicos, registrando erro caso permaneça utilizando aparelho conectado à rede elétrica. | Importante |
| RF045 | O sistema deve registrar erro grave caso o usuário suba ao telhado ou tente reparar a cobertura enquanto a tempestade estiver em curso. | Essencial |
| RF046 | O sistema deve simular infiltração ou alagamento localizado decorrente do acúmulo de granizo em calhas e ralos obstruídos, exigindo do usuário a proteção de móveis e equipamentos nas áreas atingidas. | Desejável |
| RF047 | O sistema deve encerrar a precipitação após o intervalo definido para o cenário e simular os danos resultantes: telhas quebradas, vidros estilhaçados, galhos e fiação caídos, pedras de gelo acumuladas. | Essencial |
| RF048 | O sistema deve registrar erro grave caso o usuário toque em fiação caída ou em poças em contato com cabos energizados na fase posterior à tempestade. | Essencial |
| RF049 | O sistema deve exigir, na fase posterior à tempestade, a vistoria dos danos e a cobertura provisória das áreas comprometidas do telhado, permitida apenas após o encerramento da precipitação. | Importante |
| RF050 | O sistema deve concluir a fase com sucesso quando o usuário tiver permanecido abrigado durante toda a precipitação e concluído a vistoria posterior sem incorrer em erro grave. | Essencial |
| RF051 | O sistema deve apresentar, no relatório final, esclarecimento quanto ao risco de abrigar-se sob árvores durante tempestades, caso o usuário adote esse comportamento. | Desejável |

### 3.6 Fase 3 — Incêndio

**Contexto:** princípio de incêndio iniciado na cozinha da residência, com propagação de fogo e fumaça ao longo do tempo. A saída pela porta principal é a rota de fuga primária e o portão da frente é o ponto de encontro.

| ID | Requisito | Prioridade |
|---|---|---|
| RF052 | O sistema deve simular a propagação progressiva do fogo e o acúmulo de fumaça, reduzindo a visibilidade de cima para baixo ao longo do tempo. | Essencial |
| RF053 | O sistema deve exigir, como evento crítico, o alerta aos demais moradores da residência e o acionamento do serviço de emergência (193) a partir do telefone do cenário, após a saída do imóvel. | Essencial |
| RF054 | O sistema deve permitir o uso de extintor de incêndio conforme o mnemônico PASS, exigindo a sequência correta de puxar o pino, apontar para a base do fogo, apertar o gatilho e varrer a base. | Essencial |
| RF055 | O sistema deve disponibilizar extintores de classes distintas e registrar erro grave caso o usuário empregue extintor à base de água sobre fogo de origem elétrica ou em líquidos inflamáveis. | Essencial |
| RF056 | O sistema deve permitir o combate ao foco apenas enquanto este for classificado como princípio de incêndio, e registrar erro grave caso o usuário insista em combater foco já propagado em vez de evacuar. | Essencial |
| RF057 | O sistema deve exigir que o usuário verifique a temperatura da porta com o dorso da mão antes de abri-la, e registrar erro grave caso abra porta aquecida com fogo do outro lado. | Essencial |
| RF058 | O sistema deve exigir o deslocamento agachado nas áreas com fumaça, reduzindo a integridade do usuário caso ele permaneça em pé na camada de fumaça. | Essencial |
| RF059 | O sistema deve registrar erro grave caso o usuário retorne ao interior da residência após ter saído, seja para buscar pertences, documentos ou animais. | Essencial |
| RF060 | O sistema deve prever ao menos duas rotas de saída da residência (porta frontal e saída pelos fundos ou janela térrea acessível), exigindo que o usuário identifique e utilize uma rota válida sem sinalização prévia, à semelhança de um plano de fuga doméstico. | Essencial |
| RF061 | O sistema deve simular situação em que a rota principal esteja bloqueada, exigindo o uso de rota alternativa. | Importante |
| RF062 | O sistema deve prever situação de fogo nas vestimentas do usuário, exigindo a execução da sequência **parar, deitar e rolar**. | Importante |
| RF063 | O sistema deve permitir a vedação de frestas de porta com tecido úmido quando a evacuação estiver inviabilizada, como procedimento de contenção de fumaça. | Desejável |
| RF064 | O sistema deve concluir a fase com sucesso quando o usuário alcançar o ponto de encontro externo. | Essencial |

### 3.7 Módulo — Avaliação e relatório

| ID | Requisito | Prioridade |
|---|---|---|
| RF065 | O sistema deve registrar, durante a execução da fase, todas as ações relevantes do usuário, com identificação da ação, instante de ocorrência e classificação (acerto, erro leve ou erro grave). | Essencial |
| RF066 | O sistema deve calcular a pontuação final da fase conforme as regras definidas em RN01 a RN05. | Essencial |
| RF067 | O sistema deve exibir, ao término da fase, um relatório contendo: resultado (sucesso ou falha), pontuação, tempo de conclusão, lista de acertos e lista de erros. | Essencial |
| RF068 | O sistema deve apresentar, para cada erro cometido, explicação textual do procedimento correto e da razão pela qual a ação adotada é inadequada. | Essencial |
| RF069 | O sistema deve exibir a classificação de desempenho obtida, conforme a faixa de pontuação definida em RN05. | Importante |
| RF070 | O sistema deve armazenar localmente o resultado de cada tentativa, preservando a melhor pontuação por fase. | Essencial |
| RF071 | O sistema deve permitir a exportação do histórico de resultados em formato legível por máquina, para uso do instrutor. | Desejável |

---

## 4. Regras de negócio

| ID | Regra |
|---|---|
| RN01 | Cada fase inicia com 100 pontos. |
| RN02 | Cada erro leve (procedimento subótimo, omissão de item não essencial) subtrai 5 pontos. |
| RN03 | Cada erro grave (ação que, na realidade, implicaria risco de morte) subtrai 25 pontos. |
| RN04 | A conclusão da fase acima do tempo-limite definido para o cenário subtrai 10 pontos; o tempo-limite não encerra a fase por si só. |
| RN05 | A classificação de desempenho segue as faixas: 90–100 pontos, Excelente; 70–89, Adequado; 50–69, Requer revisão; abaixo de 50, Insuficiente — com recomendação de repetição da fase. |
| RN06 | Após 30 segundos de inatividade do usuário, o sistema exibe orientação contextual; após 60 segundos, exibe orientação explícita quanto à próxima ação esperada. |
| RN07 | A fase é encerrada em condição de falha quando a integridade do usuário chega a zero ou quando é cometido erro grave classificado como fatal no cenário. |
| RN08 | Erros graves permanecem registrados no relatório ainda que o usuário execute posteriormente o procedimento correto. |
| RN09 | A repetição da fase não apaga o histórico de tentativas anteriores; apenas a melhor pontuação é destacada. |

---

## 5. Requisitos não funcionais

### 5.1 Desempenho

| ID | Requisito |
|---|---|
| RNF01 | A aplicação deve manter no mínimo 72 quadros por segundo em execução autônoma no HMD-alvo, sem quedas sustentadas abaixo desse valor. **[CONFIRMAR — 90 fps caso o alvo seja PC VR]** |
| RNF02 | A latência entre o movimento da cabeça do usuário e a atualização da imagem exibida deve permanecer abaixo de 20 ms. |
| RNF03 | O carregamento de uma fase não deve exceder 15 segundos, exibindo-se indicação de progresso em ambiente estático durante a espera. |
| RNF04 | Os efeitos de água, fogo e fumaça devem ser implementados com técnicas compatíveis com o orçamento gráfico do dispositivo autônomo, sem comprometer o requisito RNF01. |

### 5.2 Usabilidade e conforto

| ID | Requisito |
|---|---|
| RNF05 | Todas as configurações de conforto (RF008 a RF012) devem ser acessíveis tanto no menu principal quanto durante a fase, sem perda do progresso. |
| RNF06 | O sistema não deve, em nenhuma circunstância, mover a câmera do usuário sem comando dele, nem aplicar aceleração ou rotação forçada do ponto de vista. |
| RNF07 | Cada fase deve ser concluível em até 10 minutos por usuário de desempenho mediano, limitando a exposição contínua ao HMD. |
| RNF08 | Textos exibidos em ambiente virtual devem ser legíveis a partir da distância típica de interação, com corpo e contraste adequados à resolução do HMD-alvo. |
| RNF09 | Elementos de interface fixados ao campo de visão devem ser posicionados em profundidade confortável, evitando conflito de convergência e acomodação. |
| RNF10 | Um usuário sem experiência prévia em VR deve concluir o tutorial e iniciar a primeira fase sem auxílio externo. |

### 5.3 Acessibilidade

| ID | Requisito |
|---|---|
| RNF11 | Toda informação transmitida por áudio deve possuir equivalente visual (legenda ou ícone). |
| RNF12 | Nenhuma informação essencial deve ser transmitida exclusivamente por cor. |
| RNF13 | Todas as ações do jogo devem ser executáveis sem exigir que o usuário se agache fisicamente, disponibilizando-se alternativa por comando do controle. |
| RNF14 | O sistema deve permitir a redução da intensidade dos efeitos de cintilação, relâmpagos, tremulação de imagem e transições bruscas, para usuários sensíveis. |

### 5.4 Segurança do usuário

| ID | Requisito |
|---|---|
| RNF15 | O sistema deve exibir, antes da primeira sessão, aviso quanto a riscos de desconforto, à necessidade de área física livre e à orientação de interromper o uso em caso de mal-estar. |
| RNF16 | O sistema deve disponibilizar interrupção imediata da simulação por comando único do controle, com retorno a ambiente neutro. |
| RNF17 | O sistema deve sugerir pausa após duas fases consecutivas concluídas na mesma sessão. |
| RNF18 | O sistema deve pausar automaticamente a simulação quando o usuário retirar o HMD. |

### 5.5 Confiabilidade e manutenibilidade

| ID | Requisito |
|---|---|
| RNF19 | Os parâmetros de cada fase (tempo-limite, taxa de subida da água, velocidade de propagação do fogo, intensidade e duração da precipitação de granizo, pontuações) devem ser externalizados em arquivos de configuração ou objetos parametrizáveis, sem necessidade de alteração de código. |
| RNF20 | O sistema não deve depender de conexão de rede para nenhuma de suas funcionalidades. |
| RNF21 | A falha de carregamento de um recurso não deve encerrar a aplicação; o sistema deve retornar ao menu principal com mensagem de erro. |
| RNF22 | A arquitetura deve permitir a inclusão de novas fases mediante reaproveitamento dos módulos de interação, avaliação e relatório. |
| RNF23 | O código-fonte deve ser versionado em repositório Git, com histórico de commits descritivo. |

### 5.6 Portabilidade

| ID | Requisito |
|---|---|
| RNF24 | A aplicação deve ser construída sobre o padrão OpenXR, de modo a permitir a execução em dispositivos de fabricantes distintos com alteração mínima de código. **[CONFIRMAR]** |
| RNF25 | O mapeamento de controles deve ser abstraído por camada de entrada, sem dependência direta de um modelo específico de controle. |

### 5.7 Localização

| ID | Requisito |
|---|---|
| RNF26 | Todo o conteúdo textual e sonoro deve ser apresentado em português brasileiro. |
| RNF27 | Os textos devem ser mantidos em arquivos de recurso externos ao código, viabilizando tradução futura. |

---

## 6. Casos de uso principais

### CSU01 — Executar fase de emergência

- **Ator:** Treinando
- **Pré-condições:** Tutorial concluído ou dispensado; HMD posicionado e limites de segurança configurados.
- **Fluxo principal:**
  1. O treinando seleciona uma fase no menu principal.
  2. O sistema carrega o cenário e apresenta a situação inicial.
  3. O sistema inicia a simulação do evento de emergência e a contagem de tempo.
  4. O treinando executa ações de proteção e evacuação.
  5. O sistema registra e classifica cada ação relevante.
  6. O treinando alcança o ponto seguro do cenário.
  7. O sistema encerra a fase, calcula a pontuação e exibe o relatório de desempenho.
- **Fluxos alternativos:**
  - 4a. O treinando permanece inativo além do limite de RN06 → o sistema exibe orientação contextual.
  - 4b. O treinando comete erro grave fatal → o sistema encerra a fase em condição de falha e exibe o relatório (RN07).
  - 4c. O treinando interrompe a sessão → o sistema descarta a tentativa e retorna ao menu (RF003).
- **Pós-condições:** Resultado da tentativa registrado no histórico local.

### CSU02 — Configurar conforto

- **Ator:** Treinando
- **Fluxo principal:** o treinando acessa as configurações, ajusta modo de locomoção, tipo de rotação, vinheta, altura do ponto de vista e volumes; o sistema aplica as alterações imediatamente e as persiste para sessões futuras.

### CSU03 — Consultar desempenho

- **Ator:** Treinando ou instrutor
- **Fluxo principal:** o ator acessa o histórico no menu principal; o sistema apresenta as tentativas registradas com fase, data, pontuação e tempo, permitindo abrir o relatório detalhado de cada uma.

---

## 7. Matriz de rastreabilidade (objetivos × requisitos)

| Objetivo | Requisitos relacionados |
|---|---|
| OBJ01 | RF022–RF064, RF018 |
| OBJ02 | RF025, RF029, RF036, RF042, RF048, RF056, RF058, RNF15–RNF18 |
| OBJ03 | RF065–RF071, RN01–RN09 |
| OBJ04 | RF029, RF037, RF045, RF051, RF055, RF057, RF059, RF068 |

---

## 8. Evolução futura

| ID | Item |
|---|---|
| EV01 | Novas fases: deslizamento de terra, descargas atmosféricas, acidente com produtos perigosos. |
| EV02 | Modo cooperativo com múltiplos usuários no mesmo cenário. |
| EV03 | Painel web para instrutores, com consolidação de resultados de turmas. |
| EV04 | Registro e reprodução da trajetória do usuário para análise posterior. |
| EV05 | Variação procedural do cenário a cada tentativa, evitando memorização do percurso. |
| EV06 | Suporte a rastreamento de mãos sem controles. |

---

## 9. Histórico de revisões

| Versão | Data | Autor | Descrição |
|---|---|---|---|
| 1.0 | 10/09/2026 | Dai Oliveira | Versão inicial, cobrindo as fases de alagamento, granizo e incêndio. |

# SafeZone-VR

**Simulador em realidade virtual para treinar, na prática, o que fazer em três emergências: alagamento, tempestade de granizo e incêndio.**

O jogador vive a situação dentro de uma residência unifamiliar, executa as ações de autoproteção e evacuação com as próprias mãos e, no fim de cada fase, recebe um relatório de desempenho com o que acertou, o que errou e por quê. O conteúdo segue as orientações públicas da Defesa Civil e do Corpo de Bombeiros.

## Por que este projeto existe

As cartilhas da Defesa Civil existem e são boas, mas quase ninguém as lê antes da emergência, e menos gente ainda lembra delas na hora do susto. Em vez de _ler_ sobre o que fazer, o SafeZone-VR faz a pessoa **praticar** as decisões corretas em um ambiente seguro.

### Objetivos

| ID    | Objetivo                                                                                                                                              |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| OBJ01 | Ensinar, por vivência prática simulada, os procedimentos corretos diante de alagamento, granizo e incêndio.                                           |
| OBJ02 | Permitir a prática de situações de risco sem exposição física real.                                                                                   |
| OBJ03 | Dar retorno objetivo sobre os acertos e erros cometidos durante a simulação.                                                                          |
| OBJ04 | Corrigir concepções equivocadas frequentes: voltar para dentro de imóvel em chamas, atravessar área alagada, abrigar-se sob árvore durante o granizo. |

O jogo ensina pela ação, não pelo susto: sem sangue, sem ferimentos graves, com foco na atitude correta.

---

## As três fases

As três fases reaproveitam o mesmo cenário-base (uma **casa térrea** com quintal, garagem e telhado de telhas cerâmicas, mais a praça do outro lado da rua), variando o evento, os danos e os objetos interativos.

### Fase 1 — Alagamento

A casa é atingida por uma enchente e o nível da água sobe progressivamente. O ponto de encontro elevado, na calçada em frente à casa, é o refúgio seguro.

- **Evento crítico:** desligar o quadro de disjuntores antes que a água atinja tomadas e equipamentos elétricos.
- **Evento crítico:** coletar os itens essenciais do kit de emergência (documentos, água potável, lanterna, medicamentos) antes de evacuar.
- **Erro grave:** tocar em equipamento elétrico energizado estando na água encerra a fase em falha.
- **Mecânica:** a água reduz a velocidade do jogador proporcionalmente à profundidade.
- **Sucesso:** alcançar o pavimento superior ou o ponto elevado seguro.

### Fase 2 — Granizo

O jogador começa na praça, sob alerta meteorológico iminente. Deve orientar o vizinho sobre onde estacionar, conduzir um idoso com andador até a UBS da esquina, permanecer abrigado durante a precipitação e, ao final, voltar para casa e vistoriar os danos.

- **Sinais de aproximação:** céu escurecendo, névoa e alerta textual da Defesa Civil.
- **Precipitação:** granizo de intensidade crescente, com efeito visual e sonoro e acúmulo de pedras de gelo no chão.
- **Erros graves:** abrigar-se sob árvore, pergolado de ripas, ponto de ônibus metálico ou barraca frágil; subir ao telhado durante a tempestade.
- **Danos após a chuva:** telhas quebradas no quintal, forro cedendo com goteira no quarto.
- **Sucesso:** permanecer abrigado, vistoriar os danos e comunicar a ocorrência à Defesa Civil.

### Fase 3 — Incêndio

Um princípio de incêndio começa na cozinha, com propagação de fogo e fumaça ao longo do tempo. A porta principal é a rota de fuga e o portão da frente é o ponto de encontro.

- **Fumaça:** reduz a visibilidade de cima para baixo; é preciso se deslocar agachado, ou a integridade do jogador cai.
- **Evento crítico:** alertar os moradores e ligar para o 193 pelo telefone do cenário, depois de sair do imóvel.
- **Combate ao fogo:** só é possível enquanto for princípio de incêndio; insistir em foco já propagado é erro grave.
- **Erro grave:** voltar ao interior da casa depois de sair, seja para buscar pertences, documentos ou animais.
- **Sucesso:** alcançar o ponto de encontro externo.

---

## Avaliação e relatório

Cada ação relevante é registrada com instante e classificação (acerto, erro leve ou erro grave).

| Regra | Descrição                                                                                                                 |
| ----- | ------------------------------------------------------------------------------------------------------------------------- |
| RN01  | Cada fase começa com **100 pontos**.                                                                                      |
| RN02  | Erro leve: **-5** pontos.                                                                                                 |
| RN03  | Erro grave (ação que, na realidade, implicaria risco de morte): **-25** pontos.                                           |
| RN04  | Concluir acima do tempo-limite do cenário: **-10** pontos (o tempo não encerra a fase).                                   |
| RN05  | **90-100** Excelente · **70-89** Adequado · **50-69** Requer revisão · **abaixo de 50** Insuficiente (recomenda repetir). |
| RN06  | Após **30 s** de inatividade, dica contextual; após **60 s**, orientação explícita da próxima ação.                       |
| RN07  | A fase termina em falha se a integridade chegar a zero ou se ocorrer erro grave fatal.                                    |
| RN08  | Erros graves continuam no relatório mesmo que o jogador corrija depois.                                                   |

O relatório final mostra resultado, pontuação, tempo, lista de acertos e erros, a explicação do procedimento correto para cada erro e os _cards_ educativos da Defesa Civil. A **melhor pontuação e o melhor tempo** de cada fase ficam salvos no dispositivo.

---

## Conforto e acessibilidade

Quem passa mal não aprende, então o conforto é requisito, não enfeite.

- **Locomoção:** teleporte (padrão) ou contínua.
- **Rotação:** por incrementos (padrão) ou suave.
- **Vinheta de conforto:** ativa por padrão.
- **Mão dominante:** espelha as funções dos controles.
- **Volumes:** efeitos, narração e trilha ambiente ajustáveis separadamente.
- **Sem agachar de verdade:** toda ação pode ser feita por comando do controle.
- **Câmera nunca é movida sem comando do jogador**, sem aceleração nem rotação forçada.
- **Aviso de segurança** antes da primeira sessão (desconforto, área livre, interromper em caso de mal-estar).
- **Sair a qualquer momento** pelo painel de opções no relógio de pulso, voltando ao hub.
- Tudo em **português brasileiro** e **100% offline**.

---

## O que está (e o que não está) no escopo

**Nesta versão:** três fases independentes, interação em VR, hub 3D de seleção de fase, avaliação com feedback educativo e registro local da melhor tentativa.

**Fora do escopo:** modo multiusuário, backend ou ranking online, versão sem VR (_flat screen_), fases extras, integração com LMS/SCORM, tutorial guiado (substituído por briefing por fase e dicas por inatividade) e histórico de tentativas.

**Evolução futura:** deslizamento de terra, descargas atmosféricas e acidente químico; modo cooperativo; painel web para instrutores; gravação da trajetória; variação procedural do cenário; rastreamento de mãos.

---

## Tecnologia

| Item               | Escolha                                                                   |
| ------------------ | ------------------------------------------------------------------------- |
| Motor              | Unity **6000.5.10f1** com Universal Render Pipeline                       |
| XR                 | XR Interaction Toolkit 3.5.1 sobre OpenXR 1.17.1 (Meta OpenXR)            |
| Plataforma-alvo    | **Meta Quest 3 / 3S**, execução autônoma. PC VR **não** está configurado. |
| Compilação         | IL2CPP, ARM64, Vulkan, espaço de cor linear                               |
| Meta de desempenho | 72 fps                                                                    |
| Persistência       | `PlayerPrefs` no dispositivo                                              |
| Distribuição       | APK instalado por _sideload_ (`adb install`)                              |
| Controles          | Dois controles 6DoF, sem teclado ou mouse                                 |

O conteúdo de cada fase (cenário, passos, textos, cards e metas de tempo) fica em `ScriptableObject`s, então criar uma fase nova é escrever conteúdo, não reescrever sistemas.

> **Validação pendente:** as metas de desempenho e conforto (RNF01, RNF02, RNF04, RNF07, RNF08 e RNF09) ainda não foram medidas no Quest 3. Até agora a validação foi feita apenas no editor.

---

## Estrutura do repositório

```
game-vr-defesa-civil/
├── ERS-SafeZone-VR.md                     # Especificação de requisitos (fonte de verdade)
├── Documentos_Cenarios/                   # Roteiros e referência técnica das fases
├── Documentos_Orientações_Defesa_Civil/   # Cartilhas oficiais que embasam o conteúdo
└── UnityProject/
    ├── Assets/
    │   ├── Casa/ Praca/ Itens/ NPC/       # Modelos 3D
    │   ├── Scenes/                        # MainMenu + Fases/ (Alagamento_EmCasa, Granizo_Praca, Incendio_EmCasa)
    │   └── SafeZoneVR/
    │       ├── Data/                      # ScriptableObjects: cenários, missões, cards
    │       ├── Editor/                    # Geradores de cena, configuração do Quest e build do APK
    │       └── Runtime/                   # Core, Interactables, UI e Validators
    ├── Packages/
    └── ProjectSettings/
```

---

## Como abrir o projeto

1. Instale o **Unity 6000.5.10f1** pelo Unity Hub, com o módulo **Android Build Support** (SDK, NDK e OpenJDK).
2. Abra a pasta `UnityProject/` pelo Unity Hub. A primeira abertura demora, porque a pasta `Library/` é gerada localmente e não vai para o Git.
3. Abra `Assets/Scenes/MainMenu.unity` ou uma fase em `Assets/Scenes/Fases/`. Sem headset, o **XR Interaction Simulator** é criado automaticamente ao dar Play.
4. Para regenerar dados, fases e menu: **SafeZone VR > Build > 0. Montar tudo** (detalhes em [CONFIGURATION_GUIDE.md](UnityProject/Assets/SafeZoneVR/CONFIGURATION_GUIDE.md)).

---

## Como gerar e instalar o APK (Meta Quest)

1. **SafeZone VR > Build > 4. Configurações do projeto (Quest 3)** aplica as configurações exigidas pelo APK (também roda automaticamente antes de cada build).
2. **SafeZone VR > Build > 5. Gerar APK (release)**. O arquivo sai em `UnityProject/Builds/SafeZoneVR-<versão>-release.apk` (a pasta `Builds/` não vai para o Git). Para investigar desempenho, use o **5b** com o Profiler.
3. Com o Quest em modo desenvolvedor:

```bash
adb install -r Builds/SafeZoneVR-1.0-release.apk
```

Build por linha de comando (com o Unity fechado):

```bash
Unity.exe -quit -batchmode -projectPath UnityProject -executeMethod SafeZoneVR.Editor.SafeZoneApkBuilder.BuildFromCommandLine
```

O menu **4b. Conferir configurações do APK** imprime no Console o que está valendo.

O APK é assinado com o keystore de debug, suficiente para _sideload_. Para publicar na Meta Store é preciso criar um keystore próprio em `Edit > Project Settings > Player > Publishing Settings` e guardar a senha fora do repositório.

---

## Documentação do projeto

| Documento                                                                   | Conteúdo                                                                |
| --------------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| [Especificacao-Requisitos.md](ERS-SafeZone-VR.md)                           | Requisitos funcionais, não funcionais, regras de negócio e casos de uso |
| [Documentos_Orientações_Defesa_Civil/](Documentos_Orientações_Defesa_Civil) | Documentações da Defesa Civil sobre como agir em cada cenário           |

---

## Fontes e aviso

As orientações do jogo vêm de material público da **Defesa Civil**, do **Ministério do Desenvolvimento Regional**, do **Corpo de Bombeiros Militar** e do sistema de alertas do **INMET** e da Defesa Civil de Santa Catarina. Este é um projeto educativo, sem fins lucrativos, e **não substitui as instruções das autoridades durante uma emergência real**.

**Em caso de emergência: Defesa Civil 199 · Bombeiros 193 · SAMU 192**

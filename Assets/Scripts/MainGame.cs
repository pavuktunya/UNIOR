using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainGame : MonoBehaviour
{
    [SerializeField] StartLevel _startlevel;
    [SerializeField] StationContent stationcontent;
    [SerializeField] ControllerManager controllerManager;
    [SerializeField] Button buttonPauseContinue;
    [SerializeField] Button buttonPlayGame;
    [SerializeField] Button buttonOneStep;
    [SerializeField] GameObject panelPause;
    public static string? GameLevelStart = null;
    public static string? GameLevelFinish = null;
    public static int? IndGameLevel=null;
    public static bool SetLevel;
    private int amountCellStateAnimations=0;
    public static Action<int> MovingCells;
    public static Action<GameMode> MainGameMode;
    private GameMode NowGameMode;
    private StationContent.Base Base;
    private int ind_state=0;
    private int ind_symbol=0;
    private string stationChosenName="Q1";
    private GameObject cell;
    private bool getcell=false;
    private GameMode gameModePause;
    private bool isContinue = true;

    [SerializeField] private Button[] button_test = new Button[4];
    [SerializeField] private GameObject _interface;
    [SerializeField] private Button ExitMenu;

    private void GetTableStates(StationContent.Base @base)
    {
        Base= @base;
        stationChosenName = Base.GetFirstState();
    }
    private void Awake()
    {
        SetLevel = true;
    }
    private void Start()
    {
        _interface.SetActive(true);
        _startlevel.SeeTable();
        NowGameMode = GameMode.Wait;
        gameModePause = GameMode.Wait;
        AddListeners();
        buttonPlayGame.onClick.AddListener(() => { ButPlay(); });
        panelPause.SetActive(false);
        if (MainGame.IndGameLevel != null)
        {
            foreach (Item item in ControllerManager.Items)
            {
                if (item.Id == IndGameLevel)
                {
                    Debug.Log("SetLevel1");
                    button_test[0].onClick.AddListener(() => { StartGame(item.Test1_StartWord, item.Test1_FinishWord); });
                    button_test[1].onClick.AddListener(() => { StartGame(item.Test2_StartWord, item.Test2_FinishWord); });
                    button_test[2].onClick.AddListener(() => { StartGame(item.Test3_StartWord, item.Test3_FinishWord); });
                    break;
                }
            }
        }
        else if (MainGame.GameLevelFinish != null && MainGame.GameLevelStart != null)
        {
            Debug.Log("SetLevel2");
            button_test[0].onClick.AddListener(() => { controllerManager.SetLevel((string)MainGame.GameLevelStart, (string)MainGame.GameLevelFinish); _startlevel.HideTable(); });
            button_test[0].gameObject.SetActive(true);
            button_test[1].gameObject.SetActive(false);
            button_test[2].gameObject.SetActive(false);
            button_test[3].gameObject.SetActive(true);
            button_test[3].transform.position = button_test[1].transform.position;
        }

        //PanelLoseWin.GetComponent<Image>().sprite;
        //PanelLoseWin.GetComponent<Button>().onClick.AddListener(() => { });
        ExitMenu.onClick.AddListener(() => { ClearAndExit(); });


    }
    public void StartGame(string StartWord, string FinishWord)
    {
        buttonPlayGame.onClick.RemoveAllListeners();
        buttonPlayGame.onClick.AddListener(() => { ButPlay(); });
        controllerManager.SetLevel(StartWord, FinishWord); _startlevel.HideTable(); stationcontent.ClearExeption(); NowGameMode = GameMode.Wait;
        gameModePause = GameMode.Wait;
    }
    private void ClearAndExit()
    {
        MainGameMode?.Invoke(GameMode.ClearAndExit);
        _startlevel.SeeTable();
        Clear();
    }
    private void Clear()
    {
        amountCellStateAnimations = 0;
        ind_state = 0;
        ind_symbol = 0;
        NowGameMode = GameMode.Wait;
        getcell = false;
        MainGame.SetLevel = true;
        Continue();
        AddListeners();
    }
    private void ButPlay()
    {
        Debug.Log("CCC");
        if (NowGameMode == GameMode.Ecxeption)
        {
            MainGameMode?.Invoke(GameMode.RestartEcxept); NowGameMode = GameMode.Wait; AddListeners();
        }
        if (NowGameMode == GameMode.PlayWithoutPlayer)
        {
            return;
        }
        else
        {
            if (NowGameMode == GameMode.Pause)
            {
                NowGameMode = GameMode.Wait;
                Continue();
            }
            else if (!SetLevel)
            {
                NowGameMode = GameMode.PlayWithoutPlayer;
                StartPlay();
                OneStep(stationChosenName);
                Debug.Log(NowGameMode);
            }
        }
    }
    private void ButPause()
    {
        if (!panelPause.activeInHierarchy)
        {
            gameModePause = NowGameMode; Pause(); panelPause.SetActive(true); NowGameMode = GameMode.Pause; MainGameMode?.Invoke(GameMode.Wait);
        }
        else
        {
            Continue(); NowGameMode = gameModePause;
        }
    }
    private void ButStep()
    {
        if (NowGameMode == GameMode.Pause)
        {
            NowGameMode = GameMode.Wait;
            Continue();
        }
        else if(NowGameMode == GameMode.PlayOneStep && isContinue)
        {
            OneStep(stationChosenName);
        }
        else if (NowGameMode == GameMode.Wait || NowGameMode == GameMode.PlayOneStep)
        {
            Continue(); 
            if (!SetLevel && NowGameMode!=GameMode.PlayOneStep)
            {
                NowGameMode = GameMode.PlayOneStep;
                StartPlay();
                OneStep(stationChosenName);
            };
        }
    }
    private void AddListeners()
    {
        isContinue = true;
        buttonOneStep.image.color = Color.white;
        buttonOneStep.onClick.RemoveAllListeners();
        buttonOneStep.onClick.AddListener(() => { ButStep(); });
        buttonPauseContinue.onClick.RemoveAllListeners();
        buttonPauseContinue.image.color = Color.white;
        buttonPauseContinue.onClick.AddListener(() => { ButPause(); });
    }
    public void Restart()
    {
        Clear();
        MainGameMode?.Invoke(GameMode.Restart);
        panelPause.SetActive(false);
    }
    private void StartPlay()
    {
        stationcontent.OnReceivedStations();
        StationContent.ActGame();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)Pause();
    }
    private void OnEnable()
    {
        StationContent.onBase += GetTableStates;
        OnAnimationOver.AnimationOver += AnimationOver;
        ControllerManager.GetCell += GetCell;
    }
    private void OnDisable()
    {
        StationContent.onBase -= GetTableStates;
        OnAnimationOver.AnimationOver -= AnimationOver;
        ControllerManager.GetCell -= GetCell;
    }

    private void GetCell(GameObject cell)
    {
        this.cell = cell;
        getcell = true;
        Debug.Log(getcell);
        if (NowGameMode == GameMode.PlayWithoutPlayer)
        {
            OneStep(stationChosenName);
        }
    }
    private void GoCellAnimation(States states)
    {
        if(cell!=null)
        this.cell.GetComponent<Animator>().SetTrigger(name: states.ToString());
    }
    private void CellRename(string NewCellName)
    {
        if(cell!=null)
        cell.transform.GetChild(0).GetComponent<TextMeshPro>().text = NewCellName;
    }
    private void AnimationOver(States cellAnimationState)
    {
        if (cellAnimationState == States.cell_rename)
        {
            isContinue = false;
            if (overwrite_symbol!="" && overwrite_symbol!="_")
            CellRename(overwrite_symbol);
            GoCellAnimation(States.cell_state);
        }
        if (cellAnimationState == States.cell_state)
        {
            amountCellStateAnimations++;
            if (amountCellStateAnimations == 1)
            {
                GoCellAnimation(States.cell_rename);
            }
            if (amountCellStateAnimations == 2)
            {
                amountCellStateAnimations = 0;
                getcell = false;
                switch (symbolTable[2])
                {
                    case "L": MoveCells(-1); break;
                    case "R": MoveCells(1); break;
                    case "N": getcell = true; break;
                }
                if (symbolTable[3] == "!")
                {
                    NowGameMode = GameMode.Finish;
                    MainGameMode?.Invoke(GameMode.Finish);
                    buttonOneStep.onClick.RemoveAllListeners();
                    buttonPlayGame.onClick.RemoveAllListeners();
                    buttonPauseContinue.onClick.RemoveAllListeners();
                }
                Debug.Log(stationChosenName+"   STATE");
                Debug.Log(overwrite_symbol + "  RENAME");
                this.stationChosenName = symbolTable[3];
                this.ind_state = stateIndex;
                this.ind_symbol = overwrite_symbolIndex;
                isContinue= true;

                if (symbolTable[2] == "N")
                {
                    if (NowGameMode == GameMode.PlayWithoutPlayer)
                    {
                        OneStep(stationChosenName);
                    }
                }
            }
        }
    }
    int stateIndex;
    string state;
    string overwrite_symbol;
    int overwrite_symbolIndex;
    string name_symbol;
    string[] symbolTable;

    private void OneStep(string searchedState)
    {
        if (getcell)
        {
            name_symbol = cell.transform.GetChild(0).GetComponent<TextMeshPro>().text;
            if (name_symbol == ""|| name_symbol==null) name_symbol = "_";
            StationContent.Base.Table table = Base.SearchSymbol(searchedState, name_symbol, ind_state, ind_symbol);
            if (table != null)
            {
                stateIndex = table.state_index;
                state = table.ReturnStateByIndex(stateIndex);
                overwrite_symbolIndex = table.symbol_index;
                symbolTable = table.ReturnSymbolByIndex(overwrite_symbolIndex);
                overwrite_symbol = symbolTable[1];
                if (overwrite_symbol == "_")
                {
                    overwrite_symbol= null;
                }
                GoCellAnimation(States.cell_state);
            }
            else
            {
                Debug.Log("ecxeption");
                MainGameMode?.Invoke(GameMode.Ecxeption);
                MainGame.SetLevel = true;
                NowGameMode=GameMode.Ecxeption;
                buttonOneStep.onClick.RemoveAllListeners();
                buttonPauseContinue.onClick.RemoveAllListeners();
                buttonOneStep.image.color = Color.gray;
                buttonPauseContinue.image.color = Color.gray;
                stationcontent.OnReceivedStations();
            }
        }
    }
    private void MoveCells(int vector)
    {
        MovingCells?.Invoke(vector);
    }
    private void Pause()
    {
        Time.timeScale = 0f;
        Debug.Log("Pause");
    }
    public void Continue()
    {
        Time.timeScale = 1f;
        panelPause.SetActive(false);
        Debug.Log("Continue");
    }
}
public enum GameMode {
    PlayWithoutPlayer,
    PlayOneStep,
    Wait,
    Pause,
    Restart,
    Ecxeption,
    RestartEcxept,
    Finish,
    ClearAndExit,
}

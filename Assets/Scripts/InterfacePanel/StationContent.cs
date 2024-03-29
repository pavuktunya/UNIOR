using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StationContent : MonoBehaviour
{
    [SerializeField] private RectTransform thisContent;
    public static RectTransform content;
    [SerializeField] private GameObject el_station;
    [SerializeField] private GameObject el_symbol;
    [SerializeField] private GameObject el_add_station;
    [SerializeField] private GameObject el_add_symbol;
    [SerializeField] private Sprite stateSprite;
    [SerializeField] private ScoreManager scoreManager;
    private static List<ElementStation> elementStations = new List<ElementStation>();
    private static List<string[][]> Symbols=new List<string[][]>();
    private static List<string> Stations = new List<string>();
    public static string[] exepshin;
    public static Base bas;
    [SerializeField] private Button butCleanTable;
    [SerializeField] private Button butSaveTable;
    [SerializeField] private Button butReturnTable;
    [SerializeField] private GameObject textSaved;
    private bool flag = false;
    ControllerManager.PullObject pull_symbols, pull_states, pull_add_states, pull_add_symbols;


    private void Start()
    {
        pull_symbols = new ControllerManager.PullObject(50, el_symbol, 10);
        pull_states = new ControllerManager.PullObject(20, el_station, 5);
        pull_add_states = new ControllerManager.PullObject(1, el_add_station, 0);
        pull_add_symbols = new ControllerManager.PullObject(20, el_add_symbol, 5);

        butCleanTable.onClick.AddListener(() => {CleanTable();});
        if (MainGame.IndGameLevel != null)
        {
            butSaveTable.onClick.AddListener(() => { SaveTable(); });
            butReturnTable.onClick.AddListener(() => { GetTable(); });
            butSaveTable.gameObject.SetActive(true);
            butReturnTable.gameObject.SetActive(true);
        }
        else
        {
            butSaveTable.gameObject.SetActive(false);
            butReturnTable.gameObject.SetActive(false);
        }
        content = thisContent;
        elementStations.Clear();
        ClearExeption();
        textSaved.SetActive(false);
        GetTable();
    }
    private void SaveTable()
    {
        if (MainGame.IndGameLevel != null)
        {
            OnReceivedStations();
            scoreManager.InputStatesAndSymbols(Stations, Symbols);
            StartCoroutine(SeeGraph(textSaved));
        }
    }
    private void GetTable()
    {
        if (MainGame.IndGameLevel != null)
        {
            if (scoreManager.GetSymbols() != null && scoreManager.GetStates() != null)
            {
                Symbols = scoreManager.GetSymbols();
                Stations = scoreManager.GetStates();
                flag = true;
                OnReceivedStations();
            }
        }
    }
    IEnumerator<WaitForSeconds> SeeGraph(GameObject @object)
    {
        textSaved.SetActive(true);
        yield return new WaitForSeconds(1f);
        textSaved.SetActive(false);
        StopCoroutine(SeeGraph(textSaved));
    }
    private void CleanTable()
    {
        Symbols.Clear(); Stations.Clear();
        ClearExeption();

        pull_add_states.ResetVoiceCount();
        pull_add_symbols.ResetVoiceCount();
        pull_states.ResetVoiceCount();
        pull_symbols.ResetVoiceCount();

        elementStations.Clear();
        OnReceivedStations();
    }
    public void ClearExeption()
    {
        exepshin = null;
        new Exepshin();
        OnReceivedStations();
    }
    private void EventTracking(GameMode _event)
    {
        switch (_event)
        {
            case GameMode.Restart:
                Restart();
                break;
            case GameMode.Ecxeption:
                RestartEcxept();
                break;
            case GameMode.RestartEcxept:
                Restart();
                break;
            case GameMode.ClearAndExit: OnReceivedStations(); break;
        }
    }
    private void Restart()
    {
        new Exepshin();
        StationContent.exepshin = null;
        OnReceivedStations();
    }
    private void RestartEcxept()
    {
        OnReceivedStations();
    }
    private void OnEnable()
    {
        MainGame.MainGameMode += EventTracking;
    }
    private void OnDisable()
    {
        MainGame.MainGameMode -= EventTracking;
    }

    public void OnReceivedStations()
    {
        GameObject Instans(ControllerManager.PullObject pull)
        {
            return pull.GetAnotherObject(content);
        }
        if (flag)
        {
            elementStations.Clear();

            for (int i=0; i<Stations.Count(); i++)
            {
                elementStations.Add(new ElementStation(i));
                elementStations[i].BeforeSetName(Stations[i]);
                foreach (string[] table_symbol in Symbols[i])
                {
                    elementStations[i].BeforeAddSymbol(table_symbol);
                }
            }
            flag = false;
        }
        else if (exepshin != null)
        {
            ActGame();
            bas.SearchSymbol(exepshin[0], exepshin[1], Int32.Parse(exepshin[2]), Int32.Parse(exepshin[3]));
        }

        Symbols.Clear(); Stations.Clear();

        pull_add_states.ResetVoiceCount();
        pull_add_symbols.ResetVoiceCount();
        pull_states.ResetVoiceCount();
        pull_symbols.ResetVoiceCount();



        if (elementStations.Count == 0)
        {
            elementStations.Add(new ElementStation(0));
            elementStations[0].AddSymbol();
        }
        else if (elementStations.Count == 1)
        {
            if (elementStations[0].symbolsElements.Count == 0)
                elementStations[0].AddSymbol();
        }


        bool isFirst = true;
        for (int i=0; i<elementStations.Count(); i++)
        {
            bool exepshin = false;
            elementStations[i].InitializeExepshin(false);
            if (Exepshin.error_state.Count() != 0 && Exepshin.error_state.Contains(i))
            {
                exepshin = true;
                elementStations[i].InitializeExepshin(exepshin);
            }
            for (int c=0;c< elementStations[i].symbolsElements.Count(); c++)
            {
                elementStations[i].symbolsElements[c].InitializeExepshin(false);
                if (isFirst &&exepshin && Exepshin.error_symbol.Count()!=0 && Exepshin.error_symbol.Contains(c))
                {
                    elementStations[i].symbolsElements[c].InitializeExepshin(exepshin);
                    isFirst = false;
                }
                elementStations[i].symbolsElements[c].Delete();
            }
            elementStations[i].Delete();
        }

        isFirst = true;
        foreach (ElementStation elementStation in elementStations)
        {
            elementStation.InitializeStation(Instans(pull_states));
            if (isFirst==true) elementStation.SetBaseSprite(stateSprite);
            isFirst= false;
            foreach (ElementSymbol symbol in elementStation.symbolsElements)
            {
                symbol.InitializeSymbol(Instans(pull_symbols));
            }
            Symbols.Add(elementStation.ReturnSymbs());
            Stations.Add(elementStation.GetName());
            elementStation.CollapseSymbols(elementStation.isActive);
            GameObject buttonAddSymbol =Instans(pull_add_symbols);
            buttonAddSymbol.SetActive(elementStation.isActive);
            buttonAddSymbol.GetComponent<Button>().onClick.RemoveAllListeners();
            buttonAddSymbol.GetComponent<Button>().onClick.AddListener(() => {elementStation.AddSymbol(); OnReceivedStations(); });
        }
        GameObject buttonAddState = Instans(pull_add_states);
        buttonAddState.GetComponent<Button>().onClick.RemoveAllListeners();
        buttonAddState.GetComponent<Button>().onClick.AddListener(() => { elementStations.Add(new ElementStation(elementStations.Count)); Stations.Add(""); OnReceivedStations(); });
    }

    private class ElementStation
    {
        private int index;
        private string name = "";
        private GameObject station;
        private TMP_InputField station_input;
        private Button collapseButton;
        public List<ElementSymbol> symbolsElements;
        public bool isActive=true;
        private bool initialize = false;
        private bool exepshin = false;

        public void Delete()
        {
            if (initialize)
            {
                GetName();
                //Destroy(station);
            }
        }
        public void AddSymbol()
        {
            var symbol = new ElementSymbol(index);
            symbolsElements.Add(symbol);
            InitializeCount();
        }
        public void AddSymbol(string[] name_symbs)
        {
            var symbol = new ElementSymbol(index);
            symbolsElements.Add(symbol);
            symbol.SetText(name_symbs);
            InitializeCount();
        }
        public void BeforeAddSymbol(string[] name_symbs)
        {
            var symbol = new ElementSymbol(index);
            symbolsElements.Add(symbol);
            symbol.BeforeSetText(name_symbs);
        }
        public void InitializeCount()
        {
            foreach (ElementSymbol symbol in symbolsElements)
            {
                symbol.SetId(symbolsElements.IndexOf(symbol));
            }
        }
        public void DeleteOnObject(int id)
        {
            symbolsElements.RemoveAt(id);
            if (symbolsElements.Count() == 0)
            {
                AddSymbol();
            }
            InitializeCount();
        }
        public string[][] ReturnSymbs()
        {
            string[][] symbols;
            {
                symbols = new string[symbolsElements.Count][];
                for (int i = 0; i < symbolsElements.Count; ++i)
                {
                    if (!initialize) { symbols[i] = new string[] { "","","",""}; }
                    else symbols[i] = symbolsElements[i].GetText();
                }
                return symbols;
            }
        }
        public void CollapseSymbols(bool bl)
        {
            foreach (ElementSymbol symbol in symbolsElements)
            {
                symbol.SetAct(bl);
            }
            collapseButton.onClick.AddListener(() => { collapseButton.onClick.RemoveAllListeners(); isActive = !bl;InitializeContent(); });
        }
        public ElementStation(int index)
        {
            symbolsElements = new List<ElementSymbol>();
            this.index = index;
        }
        public void SetName(string str="")
        {
            if (initialize)
            {
                station_input.text = str;
            }
        }
        public void BeforeSetName(string str)
        {
            name=str;
        }
        public string GetName()
        {
            if (initialize)
                name = station_input.text;
            else return "";
            return name;
        }
        public void InitializeExepshin(bool exepshin)
        {
            this.exepshin = exepshin;
        }
        public void InitializeStation(GameObject station)
        {
            this.station = station;
            if (exepshin) { station.GetComponent<ExepshinSymbol>().Exepshin(); }
            
            this.station_input = station.transform.GetChild(0).GetChild(0).GetComponent<TMP_InputField>();
            this.collapseButton = station.transform.GetChild(1).GetComponent<Button>();
            collapseButton.onClick.RemoveAllListeners();
            Button buttonDelete = station.transform.GetChild(2).GetComponent<Button>();
            buttonDelete.onClick.RemoveAllListeners();
            buttonDelete.onClick.AddListener(() => { station.transform.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners(); elementStations.RemoveAt(index);InitializeIndexes(); InitializeContent(); content.GetComponent<StationContent>().ClearExeption(); });
            this.collapseButton.gameObject.GetComponent<CollapseButton>().Collapsed(isActive);
            this.station_input.text = name;
            collapseButton.onClick.AddListener(() => { collapseButton.onClick.RemoveAllListeners(); CollapseSymbols(false); isActive = false; InitializeContent(); });
            initialize = true;
            //station.transform.GetChild(3).gameObject.SetActive(MainGame.IsPlaying);
        }
        public void SetBaseSprite(Sprite spr)
        {
            station.transform.GetComponent<Image>().sprite = spr;
            station.transform.GetChild(2).GetComponent<Image>().color = Color.red;
        }
        private void InitializeIndexes()
        {
            for (int index=0; index<elementStations.Count; index++)
            {
                elementStations[index].index = index;
                elementStations[index].RewriteIndexSymbols();
            }
        }
        public void RewriteIndexSymbols()
        {
            foreach(ElementSymbol elementSymbol in symbolsElements)
            {
                elementSymbol.RewriteIndex(index);
            }
        }
        private void InitializeContent()
        {
            StationContent.content.GetComponent<StationContent>().OnReceivedStations();
        }
    }

    private class ElementSymbol
    {
        private int id;
        private int index;
        private GameObject symbol;
        private TMP_InputField name_input;
        private TMP_InputField rename_input;
        private TMP_InputField move_input;
        private TMP_InputField nextStation;
        private Button removeButton;
        private string[] texts = {"A" ,"A", "R", "" };
        private bool initialize = false;
        private bool exepshin = false;
        public ElementSymbol(int index)
        {
            this.index = index;
        }
        public void InitializeExepshin(bool exepshin)
        {
            this.exepshin = exepshin;
        }
        public void InitializeSymbol(GameObject symbol)
        {
            this.symbol = symbol;
            if (exepshin) { symbol.GetComponent<ExepshinSymbol>().Exepshin(); symbol.GetComponent<ExepshinSymbol>().SetExeptionColor(true); }
            else { symbol.GetComponent<ExepshinSymbol>().SetExeptionColor(false); }
            this.name_input = getComponent(0);
            this.rename_input = getComponent(1);
            this.move_input = getComponent(2);
            this.nextStation = getComponent(3);
            this.removeButton = symbol.transform.GetChild(1).GetComponent<Button>();
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => { Debug.Log(getId()); elementStations[this.index].InitializeCount(); elementStations[this.index].DeleteOnObject(getId()); InitializeContent(); content.GetComponent<StationContent>().ClearExeption(); });
            SetText(null);
            TMP_InputField getComponent(int ind)
            {
                return symbol.transform.GetChild(0).GetChild(ind).GetComponent<TMP_InputField>();
            }
            initialize = true;
            //symbol.transform.GetChild(2).gameObject.SetActive(MainGame.IsPlaying);
    }
        private void InitializeContent()
        {
            StationContent.content.GetComponent<StationContent>().OnReceivedStations();
        }
        
        public void SetAct(bool bl)
        {
            if (symbol!= null)
            {
                symbol.SetActive(bl);
            }
        }
        public void RewriteIndex(int ind)
        {
            this.index = ind;
        }
        public string[] GetText()
        {
            texts[0]=name_input.text;
            texts[1] = rename_input.text;
            texts[2]=move_input.text;
            texts[3]=nextStation.text;
            return texts;
        }
        public void SetText(string[] texts)
        {
            if (texts==null) texts = this.texts;
            name_input.text = texts[0];
            rename_input.text = texts[1];
            move_input.text = texts[2];
            if (texts[3] == "") { texts[3] = elementStations[this.index].GetName(); }
            nextStation.text = texts[3];
            Debug.Log($"Texts3 {texts[3]} ");
        }
        public void BeforeSetText(string[] texts)
        {
            this.texts = texts;
            Debug.Log("Add 1 symbol");
        }
        public void Delete()
        {
            if (initialize)
            {
                GetText();
                //Destroy(symbol);
            }
            //else Destroy(symbol);
        }
        public void SetId(int id)
        {
            this.id = id;
        }
        private int getId()
        {
            return id;
        }
    }
    public class Base
    {
        private static List<string> state_names;
        private static List<string[][]> state_types;
        public static Exepshin exepshin;
        public Base(List<string> state_names, List<string[][]> state_types)
        {
            Base.state_names = state_names;
            Base.state_types = state_types;
        }
        public string GetFirstState()
        {
            return state_names[0];
        }
        public Table SearchSymbol(string name_state, string name_symbol, int ind_state, int ind_symb)
        {
            StationContent.exepshin = new string[4];
            StationContent.exepshin[0] = name_state;
            StationContent.exepshin[1] = name_symbol;
            StationContent.exepshin[2] = ind_state.ToString();
            StationContent.exepshin[3] = ind_symb.ToString();
            List<int> searched_state=new List<int>();
            for (int i = 0; i < state_names.Count; i++)
            {
                if (state_names[i] ==name_state && name_state!="") searched_state.Add(i);
            }
            if (searched_state.Count() == 1)
            {
                var symbols = state_types[searched_state[0]];

                List<int> searched_symb = new List<int>();
                for (int i = 0; i < symbols.Length; i++)
                {
                    Debug.Log(symbols[i][1]);
                    if (symbols[i][0] == name_symbol && name_symbol != "") searched_symb.Add(i);
                }
                if (searched_symb.Count() == 1)
                {
                    if (symbols[searched_symb[0]][1]!="" && symbols[searched_symb[0]][2]!= ""&& symbols[searched_symb[0]][3] != "")
                    {
                        string[] command = { "L", "R", "N" };
                        if (command.Contains(symbols[searched_symb[0]][2]))
                        {
                            exepshin = new Exepshin();
                            StationContent.exepshin = null;
                            return new Table(searched_state[0], searched_symb[0]);
                        }
                        exepshin = new Exepshin(searched_state[0], searched_symb);
                    }
                    else
                    {
                        exepshin = new Exepshin(searched_state[0],searched_symb);
                    }
                }
                else if (searched_symb.Count() == 0)
                {

                    exepshin = new Exepshin(ind_state,searched_state[0], ind_symb);
                }
                else
                {
                    exepshin = new Exepshin(searched_state[0],searched_symb);
                }

            }
            else if (searched_state.Count()==0)
            {
                exepshin = new Exepshin(ind_state, ind_symb);
            }
            else
            {
                exepshin = new Exepshin(searched_state);
            }
            return null;
        }
        public class Table
        {
            public int state_index;
            public int symbol_index;
            public Table(int state_index, int symbol_index)
            {
                Debug.Log(state_index);
                this.state_index = state_index;
                this.symbol_index = symbol_index;
            }
            public string ReturnStateByIndex(int index_state)
            {
                return state_names[index_state];
            }
            public string[] ReturnSymbolByIndex(int index_symbol)
            {
                return state_types[state_index][index_symbol];
            }
        }
    }

    public class Exepshin
    {
        public static List<int> error_state = new List<int>(); 
        public static List<int> error_symbol = new List<int>();
        private void Clean()
        {
            Exepshin.error_state.Clear();
            error_symbol.Clear();
        }
        public Exepshin()
        {
            Clean();
        }
        public Exepshin(int error_state, List<int> error_symbol)
        {
            Clean();
            Exepshin.error_state.Add(error_state);
            Exepshin.error_symbol = error_symbol;
        }
        public Exepshin(int error_state_st,int error_state_fin, int error_symbol)
        {
            Clean();
            Exepshin.error_symbol.Add(error_symbol);
            Exepshin.error_state.Add(error_state_st);
            Exepshin.error_state.Add(error_state_fin);
        }
        public Exepshin(List<int> error_state)
        {
            Clean();
            Exepshin.error_state = error_state;
        }
        public Exepshin(int state, int symbol)
        {
            Clean();
            Exepshin.error_state.Add(state);
            Exepshin.error_symbol.Add(symbol);
        }
    }
    public static Action<Base> onBase;
    public static void ActGame()
    {
        var bas = new Base(Stations,Symbols);
        Debug.Log(Stations[0]);
        Debug.Log(Symbols[0][0][0]);
        StationContent.bas=bas;
        onBase?.Invoke(bas);
    }

}
﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Timers;

public class BattleManager : MonoBehaviour
{
    public enum BattleStates
    {
        START,
        BEGIN,
        PLAYERCHOICE,
        ENEMYCHOICE,
        RESULTS,
        RESET,
        LOSE,
        WIN,
        TIE
    }

    public BasePlayer player, enemy;
    public BattleStates current_state;

    // Finisher Spells
    private BaseSpell combo_finisher_spell;
    private BaseSpell reflect_finisher_spell;
    private BaseSpell affliction_finisher_spell;

    // Complete each stage one time
    private bool start_phase;
    private bool begin_phase;
    private bool player_phase;
    private bool enemy_phase;
    private bool results_phase;
    private bool lose_phase;
    private bool win_phase;
    private bool tie_phase;

    // Selected Spells
    private BaseSpell player_choice;
    private BaseSpell enemy_choice;
    private bool player_made_choice;
    private List<int> remove_indexs;

    // Battle UI Elements
    public GameObject player_summon_image;
    public GameObject enemy_summon_image;

    public GameObject player_summon_sprite;
    public GameObject enemy_summon_sprite;

    public GameObject spell_1;
    public GameObject spell_2;
    public GameObject spell_3;
    public GameObject spell_4;
    private Vector2 position_1;
    private Vector2 position_2;
    private Vector2 position_3;
    private Vector2 position_4;
    private Vector2 off_screen;

    public GameObject toggle_page;

    public GameObject player_names;
    public GameObject player_damage_text;
    public GameObject enemy_damage_text;
    private Vector2 player_damage_text_start;
    private Vector2 enemy_damage_text_start;

    public GameObject player_burn_text;
    public GameObject enemy_burn_text;
    public GameObject player_poison_text;
    public GameObject enemy_poison_text;

    public GameObject combo_finisher;
    public GameObject reflect_finisher;
    public GameObject affliction_finisher;

    public GameObject player_hp_text;
    public GameObject enemy_hp_text;
    public GameObject player_hp_bar;
    public GameObject enemy_hp_bar;

    public GameObject left_log;
    public GameObject poison_dmg;
    public GameObject burn_dmg;
    public GameObject paralyze_dmg;
    private static UnityEngine.Color green;
    private static UnityEngine.Color red;

    public GameObject finish_window;
    private Vector2 finish_window_position;
    double start_height;

    // Effect Icons
    public Sprite burn;
    public Sprite vampire;
    public Sprite heal;
    public Sprite poison;
    public Sprite cure;
    public Sprite paralyze;

	// Use this for initialization
	void Start ()
    {
        player = GameManager.instance.player;
        enemy = GameManager.instance.enemy;
        player_summon_image.GetComponentsInChildren<Image>()[0].sprite = Load_Summon_Image(player);
        enemy_summon_image.GetComponentsInChildren<Image>()[0].sprite = Load_Summon_Image(enemy);

        player_summon_image.SetActive(false);
        enemy_summon_image.SetActive(false);

        player_summon_sprite = Load_Summon_Sprite(player);
        enemy_summon_sprite = Load_Summon_Sprite(enemy);

        player_summon_sprite = (GameObject)Instantiate(player_summon_sprite, new Vector3(-3.5f, 1.5f, -1), Quaternion.identity);
        player_summon_sprite.GetComponent<Transform>().localScale = new Vector3(3, 3, 1);
        enemy_summon_sprite = (GameObject)Instantiate(enemy_summon_sprite, new Vector3(3.5f, 1.5f, -1), Quaternion.identity);
        enemy_summon_sprite.GetComponent<Transform>().localScale = new Vector3(-3, 3, 1);

        player_made_choice = false;
        start_phase = false;
        begin_phase = false;
        player_phase = false;
        lose_phase = false;
        win_phase = false;
        tie_phase = false;

        position_1 = spell_1.transform.localPosition;
        position_2 = spell_2.transform.localPosition;
        position_3 = spell_3.transform.localPosition;
        position_4 = spell_4.transform.localPosition;
        finish_window_position = finish_window.transform.localPosition;
        off_screen = new Vector2(1000, 1000);

        Setup_Finishers();
        Setup_Spell_Font();

        green = new UnityEngine.Color(0, 214, 0);
        red = new UnityEngine.Color(255, 0, 0);

        start_height = player_hp_bar.GetComponent<Image>().rectTransform.rect.height;
        player_hp_text.GetComponent<Text>().fontSize = (int)(Screen.width * 0.018f);
        enemy_hp_text.GetComponent<Text>().fontSize = (int)(Screen.width * 0.018f);
        left_log.GetComponent<Text>().fontSize = (int)(Screen.width * 0.014f);
        poison_dmg.GetComponent<Text>().fontSize = (int)(Screen.width * 0.014f);
        burn_dmg.GetComponent<Text>().fontSize = (int)(Screen.width * 0.014f); 
        paralyze_dmg.GetComponent<Text>().fontSize = (int)(Screen.width * 0.014f);
        finish_window.GetComponentsInChildren<Text>()[0].fontSize = (int)(Screen.width * 0.03f);
        finish_window.GetComponentsInChildren<Text>()[1].fontSize = (int)(Screen.width * 0.02f);
        finish_window.GetComponentsInChildren<Text>()[2].fontSize = (int)(Screen.width * 0.02f);
        finish_window.GetComponentsInChildren<Text>()[3].fontSize = (int)(Screen.width * 0.02f);
        finish_window.GetComponentsInChildren<Text>()[4].fontSize = (int)(Screen.width * 0.02f);
        finish_window.GetComponentsInChildren<Text>()[5].fontSize = (int)(Screen.width * 0.02f);
        player_names.GetComponentsInChildren<Text>()[0].fontSize = (int)(Screen.width * 0.02f);
        string pvp = player.Player_Name + " VS. " + enemy.Player_Name;
        player_names.GetComponentsInChildren<Text>()[0].text = pvp;

        toggle_page.GetComponentsInChildren<Text>()[0].fontSize = (int)(Screen.width * 0.016f);

        finish_window.transform.localPosition = off_screen;
        Disable_Finishers();
        combo_finisher_spell = new FinalStrike();
        reflect_finisher_spell = new DamageReflection();
        affliction_finisher_spell = new GreatAffliction();
        player_damage_text_start = player_damage_text.transform.position;
        enemy_damage_text_start = enemy_damage_text.transform.position;

        current_state = BattleStates.START;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    switch (current_state)
        {
            case BattleStates.START:
                if (!start_phase)
                {
                    start_phase = true;
                    // Opening animation
                    // Player draws opening hand
                    player.Player_Spell_Book.Draw_Hand();
                    enemy.Player_Spell_Book.Draw_Hand();
                    // Display information to player
                    Display_Spells();

                    current_state = BattleStates.BEGIN;
                }
                break;
            case BattleStates.BEGIN:
                if (!begin_phase)
                {
                    begin_phase = true;

                    //player.Players_Summon.Poison = 10;
                    //player.Players_Summon.Burn = 10;

                    player_choice = null;
                    enemy_choice = null;
                    player_made_choice = false;
                    remove_indexs = new List<int>();
                    string ppd = (-Poisoned(player)).ToString();
                    string epd = (-Poisoned(enemy)).ToString();
                    string pbd = (-Burned(player)).ToString();
                    string ebd = (-Burned(enemy)).ToString();
                    poison_dmg.GetComponentInChildren<Text>().text = "Poison: " + ppd;
                    burn_dmg.GetComponentInChildren<Text>().text = "Burn: " + pbd;

                    Setup_Burn_Text(pbd, ebd);
                    Start_Burn_Text();
                    Setup_Poison_Text(ppd, epd);
                    Start_Poison_Text();

                    Display_Results();
                    if(Game_Over())
                    {
                        break;
                    }
                    Reset_Spell_Buttons();
                    current_state = BattleStates.PLAYERCHOICE;
                }
                break;
            case BattleStates.PLAYERCHOICE:
                if (!player_phase)
                {
                    player_phase = true;
                    Enable_Spells();
                    Disable_Finishers();
                    if (Paralyzed(player))
                    {
                        if (UnityEngine.Random.Range(0, 101) < 50)
                        {
                            //Player is paralyzed message
                            Debug.Log("Player is paralyzed and can't move");
                            paralyze_dmg.GetComponentInChildren<Text>().text = "Paralyze: true";
                            current_state = BattleStates.ENEMYCHOICE;
                            break;
                        }
                    }
                    paralyze_dmg.GetComponentInChildren<Text>().text = "Paralyze: false";
                    // Draw for turn         
                    left_log.GetComponentsInChildren<Text>()[0].text = "Select a spell to cast";
                    Draw();
                    Display_Spells();
                }
                break;
            case BattleStates.ENEMYCHOICE:
                if (!enemy_phase)
                {
                    enemy_phase = true;
                    if (Paralyzed(enemy))
                    {
                        if (UnityEngine.Random.Range(0, 101) < 50)
                        {
                            current_state = BattleStates.RESULTS;
                            break;
                        }
                    }
                    enemy_cast();
                    current_state = BattleStates.RESULTS;
                }
                break;
            case BattleStates.RESULTS:
                if (!results_phase)
                {
                    results_phase = true;
                    Thread.Sleep(500);
                    Calculate_Results();
                    Display_Results();
                    if(Game_Over())
                    {
                        break;
                    }
                    //current_state = BattleStates.RESET;
                }
                break;
            case BattleStates.RESET:
                begin_phase = false;
                player_phase = false;
                enemy_phase = false;
                results_phase = false;
                current_state = BattleStates.BEGIN;
                break;
            case BattleStates.LOSE:
                if (!lose_phase)
                {
                    lose_phase = true;
                    if(GameManager.instance.tournament_mode)
                    {
                        Tournament.instance.Player_Lost();
                        finish_window.GetComponentsInChildren<Button>()[0].onClick.RemoveAllListeners();
                        finish_window.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => { Return_To_Tournament(); });
                    }
                    player_summon_sprite.layer = 9;
                    enemy_summon_sprite.layer = 9;
                    Lose_Popup();         
                }
                break;
            case BattleStates.WIN:
                if (!win_phase)
                {
                    win_phase = true;
                    if (GameManager.instance.tournament_mode)
                    {
                        Tournament.instance.Player_Won();
                        finish_window.GetComponentsInChildren<Button>()[0].onClick.RemoveAllListeners();
                        finish_window.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => { Return_To_Tournament(); });
                    }
                    player_summon_sprite.layer = 9;
                    enemy_summon_sprite.layer = 9;
                    Win_Popup();
                }
                break;
            case BattleStates.TIE:
                if (!tie_phase)
                {
                    tie_phase = true;
                    if (GameManager.instance.tournament_mode)
                    {
                        Tournament.instance.Player_Lost();
                        finish_window.GetComponentsInChildren<Button>()[0].onClick.RemoveAllListeners();
                        finish_window.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => { Return_To_Tournament(); });
                    }
                    player_summon_sprite.layer = 9;
                    enemy_summon_sprite.layer = 9;
                    Tie_Popup();
                }
                break;
        }
	}

    /*
     * Player Helper functions 
     */

    private void Display_Spells()
    {
        List<GameObject> UI_spells = new List<GameObject>();
        UI_spells.Add(spell_1);
        UI_spells.Add(spell_2);
        UI_spells.Add(spell_3);
        UI_spells.Add(spell_4);
        int count = 0;
        foreach(GameObject o in UI_spells)
        {
            o.GetComponentsInChildren<Text>()[0].text = player.Player_Spell_Book.Hand[count].Name;
            o.GetComponentsInChildren<Text>()[1].text = player.Player_Spell_Book.Hand[count].Description;
            o.GetComponentsInChildren<Text>()[2].text = player.Player_Spell_Book.Hand[count].Strategy.ToString().ToLower();
            o.GetComponentsInChildren<Text>()[3].text = player.Player_Spell_Book.Hand[count].Cost.ToString();
            //switch (player.Player_Spell_Book.Hand[count].Type)
            //{
            //    case BaseSpell.
            //}
            //o.GetComponentsInChildren<Image>()[1].sprite = burn;
            count++;
        }
    }

    private void Setup_Spell_Font()
    {
        List<GameObject> UI_spells = new List<GameObject>();
        UI_spells.Add(spell_1);
        UI_spells.Add(spell_2);
        UI_spells.Add(spell_3);
        UI_spells.Add(spell_4);
        int count = 0;
        foreach (GameObject o in UI_spells)
        {
            o.GetComponentsInChildren<Text>()[0].fontSize = (int)(Screen.width * 0.014f);
            o.GetComponentsInChildren<Text>()[1].fontSize = (int)(Screen.width * 0.01f);
            o.GetComponentsInChildren<Text>()[2].fontSize = (int)(Screen.width * 0.012f);
            o.GetComponentsInChildren<Text>()[3].fontSize = (int)(Screen.width * 0.012f);
            count++;
        }
    }

    private GameObject Load_Summon_Sprite(BasePlayer p)
    {
        if (p.Players_Summon.Summon_Type == Summon.Type.DARK)
        {
            if (p.Players_Summon.Stage == 1)
            {
                return Resources.Load<GameObject>("Sprites/Animator_Prefabs/vampire_animator");
            }
            else
            {
                return Resources.Load<GameObject>("Sprites/Animator_Prefabs/vampire_evo_animator");
            }
        }
        else if (p.Players_Summon.Summon_Type == Summon.Type.EARTH)
        {
            if (p.Players_Summon.Stage == 1)
            {
                return Resources.Load<GameObject>("Sprites/Animator_Prefabs/golem_animator");
            }
            else
            {
                return Resources.Load<GameObject>("Sprites/Animator_Prefabs/golem_evo_animator");
            }
        }
        else if (p.Players_Summon.Summon_Type == Summon.Type.FIRE)
        {
            if (p.Players_Summon.Stage == 1)
            {
                return Resources.Load<GameObject>("Sprites/Animator_Prefabs/phoenix_animator");
            }
            else
            {
                return Resources.Load<GameObject>("Sprites/Animator_Prefabs/phoenix_evo_animator");
            }
        }
        else if (p.Players_Summon.Summon_Type == Summon.Type.LIGHT)
        {
            if (p.Players_Summon.Stage == 1)
            {
                return Resources.Load<GameObject>("Sprites/Animator_Prefabs/paladin_animator");
            }
            else
            {
                return Resources.Load<GameObject>("Sprites/Animator_Prefabs/paladin_evo_animator");
            }
        }
        return null;
    }

    private Sprite Load_Summon_Image(BasePlayer p)
    {
        if (p.Players_Summon.Summon_Type == Summon.Type.DARK)
        {
            if (p.Players_Summon.Stage == 1)
            {
                return Resources.Load<Sprite>("Sprites/Summon_Dark_Evo1");
            }
            else
            {
                return Resources.Load<Sprite>("Sprites/Summon_Dark_Evo2");
            }
        }
        else if (p.Players_Summon.Summon_Type == Summon.Type.EARTH)
        {
            if (p.Players_Summon.Stage == 1)
            {
                return Resources.Load<Sprite>("Sprites/Summon_Earth_Evo1");
            }
            else
            {
                return Resources.Load<Sprite>("Sprites/Summon_Earth_Evo2");
            }
        }
        else if (p.Players_Summon.Summon_Type == Summon.Type.FIRE)
        {
            if (p.Players_Summon.Stage == 1)
            {
                return Resources.Load<Sprite>("Sprites/Summon_Fire_Evo1");
            }
            else
            {
                return Resources.Load<Sprite>("Sprites/Summon_Fire_Evo2");
            }
        }
        else if (p.Players_Summon.Summon_Type == Summon.Type.LIGHT)
        {
            if (p.Players_Summon.Stage == 1)
            {
                return Resources.Load<Sprite>("Sprites/Summon_Light_Evo1");
            }
            else
            {
                return Resources.Load<Sprite>("Sprites/Summon_Light_Evo2");
            }
        }
        return null;
    }

    public void Toggle_Page()
    {
        if(spell_1.activeSelf)
        {
            Disable_Spells();
            Enable_Finishers();
            toggle_page.GetComponentsInChildren<Text>()[0].text = "Spells";
        }
        else
        {
            Enable_Spells();
            Disable_Finishers();
            toggle_page.GetComponentsInChildren<Text>()[0].text = "Finishers";
        }
    }

    public void Select_Finisher(string spell)
    {
        switch(spell)
        {
            case "combo":
                player_choice = combo_finisher_spell;
                break;
            case "reflect":
                player_choice = reflect_finisher_spell;
                break;
            case "affliction":
                player_choice = affliction_finisher_spell;
                break;
        }
        current_state = BattleStates.ENEMYCHOICE;
    }

    public void Select_Spell(string spell)
    {
        switch(spell)
        {
            case "Spell_1":
                if(player_made_choice)
                {
                    spell_1.GetComponentsInChildren<Image>()[0].color = red;
                    spell_1.GetComponentsInChildren<Button>()[0].interactable = false;
                    remove_indexs.Add(0);  
                    break;
                }
                spell_1.GetComponentsInChildren<Image>()[0].color = green;
                spell_1.GetComponentsInChildren<Button>()[0].interactable = false;
                left_log.GetComponentsInChildren<Text>()[0].text = "Select a spell to discard";
                player_choice = player.Player_Spell_Book.Hand[0];
                remove_indexs.Add(0);
                break;
            case "Spell_2":
                if (player_made_choice)
                {
                    spell_2.GetComponentsInChildren<Image>()[0].color = red;
                    spell_2.GetComponentsInChildren<Button>()[0].interactable = false;
                    remove_indexs.Add(1);              
                    break;
                }
                spell_2.GetComponentsInChildren<Image>()[0].color = green;
                spell_2.GetComponentsInChildren<Button>()[0].interactable = false;
                left_log.GetComponentsInChildren<Text>()[0].text = "Select a spell to discard";
                player_choice = player.Player_Spell_Book.Hand[1];
                remove_indexs.Add(1);
                break;
            case "Spell_3":
                if (player_made_choice)
                {
                    spell_3.GetComponentsInChildren<Image>()[0].color = red;
                    spell_3.GetComponentsInChildren<Button>()[0].interactable = false;
                    remove_indexs.Add(2);
                    break;
                }
                spell_3.GetComponentsInChildren<Image>()[0].color = green;
                spell_3.GetComponentsInChildren<Button>()[0].interactable = false;
                left_log.GetComponentsInChildren<Text>()[0].text = "Select a spell to discard";
                player_choice = player.Player_Spell_Book.Hand[2];
                remove_indexs.Add(2);
                break;
            case "Spell_4":
                if (player_made_choice)
                {
                    spell_4.GetComponentsInChildren<Image>()[0].color = red;
                    spell_4.GetComponentsInChildren<Button>()[0].interactable = false;
                    remove_indexs.Add(3);              
                    break;
                }
                spell_4.GetComponentsInChildren<Image>()[0].color = green;
                spell_4.GetComponentsInChildren<Button>()[0].interactable = false;
                left_log.GetComponentsInChildren<Text>()[0].text = "Select a spell to discard";
                player_choice = player.Player_Spell_Book.Hand[3];
                remove_indexs.Add(3);
                break;
        }
        if(player_made_choice)
        {
            remove_indexs.Sort();
            player.Player_Spell_Book.Discard(remove_indexs[0]);
            player.Player_Spell_Book.Discard(remove_indexs[1]-1);
            current_state = BattleStates.ENEMYCHOICE;
        }
        player_made_choice = true;
    }


    /*
     * Draw two new spells
     */
    private void Draw()
    {
        player.Player_Spell_Book.Draw_For_Turn();
    }


    /**
     * Battle Functions
     *
     */

    /*
     * Checks if player is paralyzed
     */
    private bool Paralyzed(BasePlayer player)
    {
        if (player.Players_Summon.Paralyze > 0)
        {
            player.Players_Summon.Paralyze--;
            return true;
        }
        return false;
    }

    private int Poisoned(BasePlayer player)
    {
        if (player.Players_Summon.Poison > 0 && player.Players_Summon.Poison_Count < player.Players_Summon.Poison)
        {
            player.Players_Summon.Poison_Count++;
            //int damage = GetNthFibonacci_Rec(player.Players_Summon.Poison_Count + player.Players_Summon.Stage);
            int damage = 2 + player.Players_Summon.Poison_Count;
            player.Players_Summon.Health = ((player.Players_Summon.Health - damage) < 0) ? 0 : player.Players_Summon.Health - damage;
            if(player.Players_Summon.Poison_Count > player.Players_Summon.Poison)
            {
                player.Players_Summon.Poison = 0;
                player.Players_Summon.Poison_Count = 0;
            }
            Debug.Log(player.Players_Summon.Name + " Poison Damage: " + damage);
            return damage;
        }
        return 0;
    }

    private int Burned(BasePlayer player)
    {
        if (player.Players_Summon.Burn > 0)
        {
            int damage = 5 * player.Players_Summon.Stage;
            player.Players_Summon.Health = ((player.Players_Summon.Health - damage) < 0) ? 0 : player.Players_Summon.Health - damage;
            player.Players_Summon.Burn--;
            return damage;
        }
        return 0;
    }

    private static int GetNthFibonacci_Rec(int n)
    {
        if ((n == 0) || (n == 1))
        {
            return n;
        }
        else
            return GetNthFibonacci_Rec(n - 1) + GetNthFibonacci_Rec(n - 2);
    }

    private void Calculate_Results()
    {
        SpellResults player_results;
        SpellResults enemy_results;
        if(player_choice == null)
        {
            player_results = new SpellResults(0, 0, 0, 0, 0, 0);
        }
        else
        {
            player_choice.Cast_Spell(player.Players_Summon);
            player_results = player_choice.Results;
        }
        if(enemy_choice == null)
        {
            enemy_results = new SpellResults(0, 0, 0, 0, 0, 0);
        }
        else
        {
            enemy_choice.Cast_Spell(enemy.Players_Summon);
            enemy_results = enemy_choice.Results;
        }

        Increment_Combo();

        int player_damaged = (player_results.Block > enemy_results.Damage) ? 0 : (enemy_results.Damage - player_results.Block);
        int enemy_damaged = (enemy_results.Block > player_results.Damage) ? 0 : (player_results.Damage - enemy_results.Block);
        int player_burned = (enemy_results.Burn > player.Players_Summon.Burn) ? enemy_results.Burn : player.Players_Summon.Burn;
        int enemy_burned = (player_results.Burn > enemy.Players_Summon.Burn) ? player_results.Burn : player.Players_Summon.Burn;
        int player_poisoned = (enemy_results.Poison > player.Players_Summon.Poison) ? enemy_results.Poison : player.Players_Summon.Poison;
        int enemy_poisoned = (player_results.Poison > enemy.Players_Summon.Poison) ? player_results.Poison : player.Players_Summon.Poison;
        int player_paralyzed = (enemy_results.Paralyze > player.Players_Summon.Paralyze) ? enemy_results.Paralyze : player.Players_Summon.Paralyze;
        int enemy_paralyzed = (player_results.Paralyze > enemy.Players_Summon.Paralyze) ? player_results.Paralyze : player.Players_Summon.Paralyze;

        player.Players_Summon.Health = ((player.Players_Summon.Health - player_damaged) < 0) ? 0 : (player.Players_Summon.Health - player_damaged);
        enemy.Players_Summon.Health = ((enemy.Players_Summon.Health - enemy_damaged) < 0) ? 0 : (enemy.Players_Summon.Health - enemy_damaged);
        player.Players_Summon.Burn = player_burned;
        enemy.Players_Summon.Burn = enemy_burned;
        player.Players_Summon.Poison = player_poisoned;
        enemy.Players_Summon.Poison = enemy_poisoned;
        player.Players_Summon.Paralyze = player_paralyzed;
        enemy.Players_Summon.Paralyze = enemy_paralyzed;
        player.Players_Summon.Health = (player.Players_Summon.Base_Health < (player_results.Heal + player.Players_Summon.Health)) ? player.Players_Summon.Base_Health : (player_results.Heal + player.Players_Summon.Health);
        enemy.Players_Summon.Health = (enemy.Players_Summon.Base_Health < (enemy_results.Heal + enemy.Players_Summon.Health)) ? enemy.Players_Summon.Base_Health : (enemy_results.Heal + enemy.Players_Summon.Health);
        Setup_Damage_Text("Damage: " + -player_damaged, "Damage: " + -enemy_damaged);

        if (player_choice != null)
        {
            if (player_choice.Type == BaseSpell.Spell_Types.ATTACK)
            {
                player_summon_sprite.GetComponent<Animator>().SetTrigger("attack");
            }
            else
            {
                player_summon_sprite.GetComponent<Animator>().SetTrigger("shield");
            }
        }
        if (enemy_choice != null)
        {
            if (enemy_choice.Type == BaseSpell.Spell_Types.ATTACK)
            {

                enemy_summon_sprite.GetComponent<Animator>().SetTrigger("attack");
            }
            else
            {
                enemy_summon_sprite.GetComponent<Animator>().SetTrigger("shield");
            }
        }

        StartCoroutine(Reset_Animator_Triggers(0.5f));
        StartCoroutine(Take_Damage_Trigger(player_damaged, enemy_damaged));

        Start_Damage_Text();
    }

    IEnumerator Take_Damage_Trigger(int player_damaged, int enemy_damaged)
    {
        yield return new WaitForSeconds(0.5f);
        if (player_damaged > 0)
        {
            player_summon_sprite.GetComponent<Animator>().SetTrigger("damage");
        }
        if (enemy_damaged > 0)
        {
            enemy_summon_sprite.GetComponent<Animator>().SetTrigger("damage");
        }
        yield return new WaitForSeconds(1.5f);
        enemy_summon_sprite.GetComponent<Animator>().ResetTrigger("damage");
        player_summon_sprite.GetComponent<Animator>().ResetTrigger("damage");
    }

    IEnumerator Reset_Animator_Triggers(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        player_summon_sprite.GetComponent<Animator>().ResetTrigger("attack");
        player_summon_sprite.GetComponent<Animator>().ResetTrigger("shield");
        player_summon_sprite.GetComponent<Animator>().ResetTrigger("damage");

        enemy_summon_sprite.GetComponent<Animator>().ResetTrigger("attack");
        enemy_summon_sprite.GetComponent<Animator>().ResetTrigger("shield");
        enemy_summon_sprite.GetComponent<Animator>().ResetTrigger("damage");
    }

    private void Increment_Combo()
    {
        if(player_choice == null)
        {
            player.Players_Summon.Combo = 0;
            player.Players_Summon.Reflect = 0;
            player.Players_Summon.Curse = 0;
            affliction_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Combo.ToString();
            combo_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Combo.ToString();
            reflect_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Reflect.ToString();
            return;
        }
        switch (player_choice.Strategy)
        {
            case BaseSpell.Spell_Class.MAGE:
                player.Players_Summon.Combo = 0;
                player.Players_Summon.Curse++;
                combo_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Combo.ToString();
                affliction_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Curse.ToString();
                break;
            case BaseSpell.Spell_Class.ROGUE:
                player.Players_Summon.Combo++;
                combo_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Combo.ToString();
                break;
            case BaseSpell.Spell_Class.WARRIOR:
                player.Players_Summon.Combo = 0;
                combo_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Combo.ToString();
                int blocked = 0;
                if(enemy_choice != null && enemy_choice.Results.Damage > 0)
                {
                    if(player_choice.Results.Block < enemy_choice.Results.Damage)
                    {
                        blocked = player_choice.Results.Block;
                    }
                    else
                    {
                        blocked = enemy_choice.Results.Damage;
                    }
                }
                player.Players_Summon.Reflect += blocked;
                reflect_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Reflect.ToString();
                break;
            case BaseSpell.Spell_Class.NONE:
                player.Players_Summon.Combo = 0;
                player.Players_Summon.Reflect = 0;
                player.Players_Summon.Curse = 0;
                affliction_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Combo.ToString();
                combo_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Combo.ToString();
                reflect_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Reflect.ToString();
                break;
        }
    }

    private void Display_Results()
    {
        player_hp_text.GetComponent<Text>().text = player.Players_Summon.Health.ToString();
        enemy_hp_text.GetComponent<Text>().text = enemy.Players_Summon.Health.ToString();

        double php = start_height - (start_height) * ((double)player.Players_Summon.Health / (double)player.Players_Summon.Base_Health);
        double ehp = start_height - (start_height) * ((double)enemy.Players_Summon.Health / (double)enemy.Players_Summon.Base_Health);
        //player_hp_bar.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(0, -(float)php);
        //enemy_hp_bar.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(0, -(float)ehp);
        StartCoroutine(Change_Player_Health(php));
        StartCoroutine(change_Enemy_Health(ehp));
    }

    IEnumerator Change_Player_Health(double php)
    {
        double current_hp = start_height - player_hp_bar.GetComponent<Image>().rectTransform.rect.height;
        int count = (int)current_hp;

        if (count <= php)
        {
            while (count < php)
            {
                player_hp_bar.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(0, -(float)count);
                count+=2;
                yield return null;
            }
        }
        else
        {
            while(count > php)
            {
                player_hp_bar.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(0, -(float)count);
                count-=2;
                yield return null;
            }
        }
    }

    IEnumerator change_Enemy_Health(double ehp)
    {
        double current_hp = start_height - enemy_hp_bar.GetComponent<Image>().rectTransform.rect.height;
        int count = (int)current_hp;

        if (count <= ehp)
        {
            while (count < ehp)
            {
                enemy_hp_bar.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(0, -(float)count);           
                count+=2;
                yield return null;
            }
        }
        else
        {
            while (count > ehp)
            {
                enemy_hp_bar.GetComponent<Image>().rectTransform.sizeDelta = new Vector2(0, -(float)count);
                count-=2;
                yield return null;
            }
        }
    }

    private void Disable_Spells()
    {
        spell_1.SetActive(false);
        spell_2.SetActive(false);
        spell_3.SetActive(false);
        spell_4.SetActive(false);
    }

    private void Enable_Spells()
    {
        spell_1.SetActive(true);
        spell_2.SetActive(true);
        spell_3.SetActive(true);
        spell_4.SetActive(true);
    }

    private void Disable_Finishers()
    {
        combo_finisher.transform.localPosition = off_screen;
        reflect_finisher.transform.localPosition = off_screen;
        affliction_finisher.transform.localPosition = off_screen;
    }

    private void Enable_Finishers()
    {
        combo_finisher.transform.localPosition = position_1;
        reflect_finisher.transform.localPosition = position_2;
        affliction_finisher.transform.localPosition = position_3;
    }

    /*
     * Dumb AI
     */
    private void enemy_cast()
    {
        int r = UnityEngine.Random.Range(0, 101);
        if(r <= 25)
        {
            enemy_choice = enemy.Player_Spell_Book.Hand[0];
        }
        else if (r <= 50)
        {
            enemy_choice = enemy.Player_Spell_Book.Hand[1];
        }
        else if (r <= 75)
        {
            enemy_choice = enemy.Player_Spell_Book.Hand[2];
        }
        else
        {
            enemy_choice = enemy.Player_Spell_Book.Hand[3];
        }
        enemy.Player_Spell_Book.Discard(0);
        enemy.Player_Spell_Book.Discard(0);

        enemy.Player_Spell_Book.Draw_For_Turn();
    }

    private void Setup_Finishers()
    {
        combo_finisher.GetComponentsInChildren<Text>()[0].fontSize = (int)(Screen.width * 0.014f);
        combo_finisher.GetComponentsInChildren<Text>()[1].fontSize = (int)(Screen.width * 0.01f);
        combo_finisher.GetComponentsInChildren<Text>()[2].fontSize = (int)(Screen.width * 0.012f);
        combo_finisher.GetComponentsInChildren<Text>()[3].fontSize = (int)(Screen.width * 0.012f);

        reflect_finisher.GetComponentsInChildren<Text>()[0].fontSize = (int)(Screen.width * 0.014f);
        reflect_finisher.GetComponentsInChildren<Text>()[1].fontSize = (int)(Screen.width * 0.01f);
        reflect_finisher.GetComponentsInChildren<Text>()[2].fontSize = (int)(Screen.width * 0.012f);
        reflect_finisher.GetComponentsInChildren<Text>()[3].fontSize = (int)(Screen.width * 0.012f);

        affliction_finisher.GetComponentsInChildren<Text>()[0].fontSize = (int)(Screen.width * 0.014f);
        affliction_finisher.GetComponentsInChildren<Text>()[1].fontSize = (int)(Screen.width * 0.01f);
        affliction_finisher.GetComponentsInChildren<Text>()[2].fontSize = (int)(Screen.width * 0.012f);
        affliction_finisher.GetComponentsInChildren<Text>()[3].fontSize = (int)(Screen.width * 0.012f);

        combo_finisher.GetComponentsInChildren<Text>()[0].text = "Combo Finisher";
        combo_finisher.GetComponentsInChildren<Text>()[1].text = "Does increased damage based on current combo";
        combo_finisher.GetComponentsInChildren<Text>()[2].text = "Combo Count";
        combo_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Combo.ToString();

        reflect_finisher.GetComponentsInChildren<Text>()[0].text = "Reflect Finisher";
        reflect_finisher.GetComponentsInChildren<Text>()[1].text = "Reflects Damage stored through defending";
        reflect_finisher.GetComponentsInChildren<Text>()[2].text = "Reflect Damage";
        reflect_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Reflect.ToString();

        affliction_finisher.GetComponentsInChildren<Text>()[0].text = "Affliction Finisher";
        affliction_finisher.GetComponentsInChildren<Text>()[1].text = "Heals for an increased amount based on affliction count";
        affliction_finisher.GetComponentsInChildren<Text>()[2].text = "Affliction Count";
        affliction_finisher.GetComponentsInChildren<Text>()[3].text = player.Players_Summon.Curse.ToString();
    }

    private void Reset_Spell_Buttons()
    {
        if (spell_1.activeSelf)
        {
            spell_1.GetComponentsInChildren<Image>()[0].color = UnityEngine.Color.white;
            spell_2.GetComponentsInChildren<Image>()[0].color = UnityEngine.Color.white;
            spell_3.GetComponentsInChildren<Image>()[0].color = UnityEngine.Color.white;
            spell_4.GetComponentsInChildren<Image>()[0].color = UnityEngine.Color.white;
            spell_1.GetComponentsInChildren<Button>()[0].interactable = true;
            spell_2.GetComponentsInChildren<Button>()[0].interactable = true;
            spell_3.GetComponentsInChildren<Button>()[0].interactable = true;
            spell_4.GetComponentsInChildren<Button>()[0].interactable = true;
        }
    }

    private bool Game_Over()
    {
        if (enemy.Players_Summon.Health <= 0 && player.Players_Summon.Health <= 0)
        {
            Disable_Spells();
            Disable_Finishers();
            current_state = BattleStates.TIE;
            return true;
        }
        else if (enemy.Players_Summon.Health <= 0)
        {
            Disable_Spells();
            Disable_Finishers();
            current_state = BattleStates.WIN;
            return true;
        }
        else if (player.Players_Summon.Health <= 0)
        {
            Disable_Spells();
            Disable_Finishers();
            current_state = BattleStates.LOSE;
            return true;
        }
        return false;
    }

    private void Win_Popup()
    {
        finish_window.GetComponentsInChildren<Text>()[0].text = "YOU WIN";
        finish_window.GetComponentsInChildren<Text>()[3].text = "100";
        finish_window.GetComponentsInChildren<Text>()[4].text = "Nothing";
        finish_window.transform.localPosition = finish_window_position;
        player.Players_Summon.Add_Experience(100);
    }

    private void Lose_Popup()
    {
        finish_window.GetComponentsInChildren<Text>()[0].text = "YOU LOSE";
        finish_window.GetComponentsInChildren<Text>()[3].text = "0";
        finish_window.GetComponentsInChildren<Text>()[4].text = "Nothing";
        finish_window.transform.localPosition = finish_window_position;
    }

    private void Tie_Popup()
    {
        finish_window.GetComponentsInChildren<Text>()[0].text = "TIE";
        finish_window.GetComponentsInChildren<Text>()[3].text = "50";
        finish_window.GetComponentsInChildren<Text>()[4].text = "Nothing";
        finish_window.transform.localPosition = finish_window_position;
        player.Players_Summon.Add_Experience(50);
    }

    public void Return_To_Main_Menu()
    {
        Clean_Up();
        GameManager.instance.current_state = GameManager.GameStates.MAIN;
        GameManager.instance.scene_loaded = false;
    }

    public void Return_To_Tournament()
    {
        Clean_Up();
        GameManager.instance.current_state = GameManager.GameStates.TOURNAMENT;
        GameManager.instance.scene_loaded = false;
    }

    private void Clean_Up()
    {
        GameManager.instance.player.Players_Summon.Health = GameManager.instance.player.Players_Summon.Base_Health;
        GameManager.instance.player.Players_Summon.Combo = 0;
        GameManager.instance.player.Players_Summon.Reflect = 0;
        GameManager.instance.player.Players_Summon.Curse = 0;
        GameManager.instance.player.Players_Summon.Burn = 0;
        GameManager.instance.player.Players_Summon.Poison = 0;
        GameManager.instance.player.Players_Summon.Poison_Count = 0;
        GameManager.instance.player.Players_Summon.Paralyze = 0;

        GameManager.instance.enemy = null;
    }

    private void Setup_Burn_Text(string player, string enemy)
    {
        Text pbt;
        Text ebt;

        player_burn_text = new GameObject();
        player_burn_text.transform.parent = GameObject.Find("Canvas").transform;
        pbt = player_burn_text.AddComponent<Text>();
        pbt.text = (player.Equals("0")) ? "" : ("Burn: " + player);  // sets it to empty if not burned
        pbt.font = Resources.Load<Font>("pixelmix");
        pbt.fontSize = (int)(Screen.width * 0.02f);
        pbt.horizontalOverflow = HorizontalWrapMode.Overflow;
        pbt.verticalOverflow = VerticalWrapMode.Overflow;
        player_burn_text.transform.localPosition = player_damage_text_start;

        enemy_burn_text = new GameObject();
        enemy_burn_text.transform.parent = GameObject.Find("Canvas").transform;
        ebt = enemy_burn_text.AddComponent<Text>();
        ebt.text = (enemy.Equals("0")) ? "" : ("Burn: " + enemy); // Sets it to empty if not burned
        ebt.font = Resources.Load<Font>("pixelmix");
        ebt.fontSize = (int)(Screen.width * 0.02f);
        ebt.horizontalOverflow = HorizontalWrapMode.Overflow;
        ebt.verticalOverflow = VerticalWrapMode.Overflow;
        enemy_burn_text.transform.localPosition = enemy_damage_text_start;
    }

    private void Start_Burn_Text()
    {
        StartCoroutine(Move_Burn_Text());
    }

    IEnumerator Move_Burn_Text()
    {
        float timer = 0;
        int count = 0;

        while (timer < 1.5f)
        {
            Vector2 position1 = new Vector2();
            position1.x = -(int)(Screen.width * 0.2f);
            position1.y = (int)(Screen.height * 0.15f * player.Players_Summon.Stage) + (count / 2);
            player_burn_text.transform.localScale = new Vector3(1, 1, 1);
            player_burn_text.transform.localPosition = position1;

            Vector2 position2 = new Vector2();
            position2.x = (int)(Screen.width * 0.18f);
            position2.y = (int)(Screen.height * 0.15f * enemy.Players_Summon.Stage) + (count / 2);
            enemy_burn_text.transform.localScale = new Vector3(1, 1, 1);
            enemy_burn_text.transform.localPosition = position2;

            count++;
            timer += Time.deltaTime;

            yield return null;
        }
        Destroy(player_burn_text);
        Destroy(enemy_burn_text);
    }

    private void Setup_Poison_Text(string player, string enemy)
    {
        Text pbt;
        Text ebt;

        player_poison_text = new GameObject();
        player_poison_text.transform.parent = GameObject.Find("Canvas").transform;
        pbt = player_poison_text.AddComponent<Text>();
        pbt.text = (player.Equals("0")) ? "" : ("Poison: " + player);  // sets it to empty if not burned
        pbt.font = Resources.Load<Font>("pixelmix");
        pbt.fontSize = (int)(Screen.width * 0.02f);
        pbt.horizontalOverflow = HorizontalWrapMode.Overflow;
        pbt.verticalOverflow = VerticalWrapMode.Overflow;
        player_poison_text.transform.localPosition = player_damage_text_start;

        enemy_poison_text = new GameObject();
        enemy_poison_text.transform.parent = GameObject.Find("Canvas").transform;
        ebt = enemy_poison_text.AddComponent<Text>();
        ebt.text = (enemy.Equals("0")) ? "" : ("Poison: " + enemy); // Sets it to empty if not burned
        ebt.font = Resources.Load<Font>("pixelmix");
        ebt.fontSize = (int)(Screen.width * 0.02f);
        ebt.horizontalOverflow = HorizontalWrapMode.Overflow;
        ebt.verticalOverflow = VerticalWrapMode.Overflow;
        enemy_poison_text.transform.localPosition = enemy_damage_text_start;
    }

    private void Start_Poison_Text()
    {
        StartCoroutine(Move_Poison_Text());
    }

    IEnumerator Move_Poison_Text()
    {
        yield return new WaitForSeconds(0.85f);
        float timer = 0;
        int count = 0;

        while (timer < 1.5f)
        {
            Vector2 position1 = new Vector2();
            position1.x = -(int)(Screen.width * 0.2f);
            position1.y = (int)(Screen.height * 0.15f * player.Players_Summon.Stage) + (count / 2);
            player_poison_text.transform.localScale = new Vector3(1, 1, 1);
            player_poison_text.transform.localPosition = position1;

            Vector2 position2 = new Vector2();
            position2.x = (int)(Screen.width * 0.18f);
            position2.y = (int)(Screen.height * 0.15f * enemy.Players_Summon.Stage) + (count / 2);
            enemy_poison_text.transform.localScale = new Vector3(1, 1, 1);
            enemy_poison_text.transform.localPosition = position2;

            count++;
            timer += Time.deltaTime;

            yield return null;
        }
        Destroy(player_poison_text);
        Destroy(enemy_poison_text);
    }

    private void Setup_Damage_Text(string player, string enemy)
    {
        Text pdt;
        Text edt;

        player_damage_text = new GameObject();
        player_damage_text.transform.parent = GameObject.Find("Canvas").transform;
        pdt = player_damage_text.AddComponent<Text>();
        pdt.text = player;
        pdt.font = Resources.Load<Font>("pixelmix");
        pdt.fontSize = (int)(Screen.width * 0.02f);
        pdt.horizontalOverflow = HorizontalWrapMode.Overflow;
        pdt.verticalOverflow = VerticalWrapMode.Overflow;
        player_damage_text.transform.localPosition = player_damage_text_start;

        enemy_damage_text = new GameObject();
        enemy_damage_text.transform.parent = GameObject.Find("Canvas").transform;
        edt = enemy_damage_text.AddComponent<Text>();
        edt.text = enemy;
        edt.font = Resources.Load<Font>("pixelmix");
        edt.fontSize = (int)(Screen.width * 0.02f);
        edt.horizontalOverflow = HorizontalWrapMode.Overflow;
        edt.verticalOverflow = VerticalWrapMode.Overflow;
        enemy_damage_text.transform.localPosition = enemy_damage_text_start;
    }

    private void Start_Damage_Text()
    {
        StartCoroutine(Move_Text());
    }

    IEnumerator Move_Text()
    {
        float timer = 0;
        int count = 0;

        while (timer < 1.5f)
        {
            Vector2 position1 = new Vector2();
            position1.x = -(int)(Screen.width * 0.2f);
            position1.y = (int)(Screen.height * 0.15f * player.Players_Summon.Stage) + (count/2);
            player_damage_text.transform.localScale = new Vector3(1, 1, 1);
            player_damage_text.transform.localPosition = position1;

            Vector2 position2 = new Vector2();
            position2.x = (int)(Screen.width * 0.18f);
            position2.y = (int)(Screen.height * 0.15f * enemy.Players_Summon.Stage) + (count/2);
            enemy_damage_text.transform.localScale = new Vector3(1, 1, 1);
            enemy_damage_text.transform.localPosition = position2;

            count++;
            timer += Time.deltaTime;

            yield return null;
        }
        Destroy(player_damage_text);
        Destroy(enemy_damage_text);
        current_state = BattleStates.RESET;
    }
    
}

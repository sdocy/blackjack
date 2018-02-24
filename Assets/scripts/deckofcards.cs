using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine;

public enum card_suit { clubs, diamonds, hearts, spades };
public enum card_rank { two, three, four, five, six, seven, eight, nine, ten, jack, queen, king, ace };

public class card_t {
    public card_suit suit;
    public card_rank rank;
    public int value;
    public Sprite pic;

    private int getCardValue(card_rank rank) {
        switch (rank) {
            case card_rank.two: return 2;
            case card_rank.three: return 3;
            case card_rank.four: return 4;
            case card_rank.five: return 5;
            case card_rank.six: return 6;
            case card_rank.seven: return 7;
            case card_rank.eight: return 8;
            case card_rank.nine: return 9;
            case card_rank.ten:
            case card_rank.jack:
            case card_rank.queen:
            case card_rank.king: return 10;
            case card_rank.ace: return 11;
        }

        return -1;
    }

    public card_t(card_suit in_suit, card_rank in_rank) {
        string card_pic;
        suit = in_suit;
        rank = in_rank;
        value = getCardValue(rank);

        card_pic = "card_images/" + rank + "_of_" + suit;
        pic = new Sprite();
        pic = Resources.Load<Sprite>(card_pic);
    }
};

public class deckofcards {

    public const int TOTAL_CARDS = 52;
    public int remaining_cards;
    public int top_card;        // array position of next card
    public card_t[] cards = new card_t[TOTAL_CARDS];

    public deckofcards() {
        int card = 0;

        foreach (card_suit suit in Enum.GetValues(typeof(card_suit)))
        {
            foreach (card_rank rank in Enum.GetValues(typeof(card_rank)))
            {
                cards[card] = new card_t(suit, rank);
                card++;
            }
        }

        remaining_cards = TOTAL_CARDS;
        top_card = 0;
    }

    private void swapCards(card_t card1, card_t card2) {
        card_suit temp_suit;
        card_rank temp_rank;
        int temp_val;
        Sprite temp_pic;

        temp_suit = card1.suit;
        temp_rank = card1.rank;
        temp_val = card1.value;
        temp_pic = card1.pic;

        card1.suit = card2.suit;
        card1.rank = card2.rank;
        card1.value = card2.value;
        card1.pic = card2.pic;

        card2.suit = temp_suit;
        card2.rank = temp_rank;
        card2.value = temp_val;
        card2.pic = temp_pic;
    }

    public void shuffleDeck() {
        int i;

        //Debug.Log("shuffling deck");

        foreach (card_t card in cards) {
            i = UnityEngine.Random.Range(0, 52);
            swapCards(card, cards[i]);
        }

        remaining_cards = TOTAL_CARDS;
        top_card = 0;

        // enable to test splitting cards
        /*cards[2].rank   = cards[0].rank;
        cards[2].value  = cards[0].value;
        cards[2].suit   = cards[0].suit;
        cards[2].pic    = cards[0].pic;
        cards[4].rank = cards[0].rank;
        cards[4].value = cards[0].value;
        cards[4].suit = cards[0].suit;
        cards[4].pic = cards[0].pic;*/
    }

    // currently assuming this can't fail
    public card_t getNextCard() {
        card_t c;

        // if (remaining_cards == 0) return error;

        c = cards[top_card];
        top_card++;
        remaining_cards--;

        return c;
    }

    public void printCard(card_t card) {
        string card_code = "";

        switch (card.rank) {
            case card_rank.two: card_code +=    "2"; break;
            case card_rank.three: card_code +=  "3"; break;
            case card_rank.four: card_code +=   "4"; break;
            case card_rank.five: card_code +=   "5"; break;
            case card_rank.six: card_code +=    "6"; break;
            case card_rank.seven: card_code +=  "7"; break;
            case card_rank.eight: card_code +=  "8"; break;
            case card_rank.nine: card_code +=   "9"; break;
            case card_rank.ten: card_code +=    "T"; break;
            case card_rank.jack: card_code +=   "J"; break;
            case card_rank.queen: card_code +=  "Q"; break;
            case card_rank.king: card_code +=   "K"; break;
            case card_rank.ace: card_code +=    "A"; break;
        }

        switch (card.suit) {
            case card_suit.clubs: card_code +=      "C"; break;
            case card_suit.diamonds: card_code +=   "D"; break;
            case card_suit.hearts: card_code +=     "H"; break;
            case card_suit.spades: card_code +=     "S"; break;
        }

        Debug.Log(card_code + " " + card.pic.name);
    }

    public void printDeck() {
        foreach (card_t card in cards) {
            printCard(card);
        }
    }

    // Use this for initialization
    void Start () {

    }
}

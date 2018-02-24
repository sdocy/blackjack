using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class blackjack : MonoBehaviour {
    const int twenty_one = 21;
    const int numCardsPerLine = 12;
    const int numCardLines = 4;
    const float dealDelay = 0.7f;
    const float dealerTurnDelay = 1.2f;
    const float resultsDelay = 1.5f;
    const float cashTextDelay = 0.007f;
    const int cashTextIters = 100;
    const int maxSplits = 3;

    public Text dealButtonText;
    public Text hitButtonText;
    public Text standButtonText;
    public Text doubleDownButtonText;
    public Text[] doubledDownText = new Text[numCardLines];
    public Text splitButtonText;
    public Text[] playerTotalText = new Text[numCardLines];
    public Text dealerTotalText;
    public Text[] winOrLoseText = new Text[numCardLines];
    public Text[] currLineTagText = new Text[numCardLines];
    public Text playerNumLivesText;
    public Text houseNumLivesText;
    public Text levelText;

    public AudioSource audioSource;
    public AudioClip shuffleAudioClip;
    public AudioClip placeCardAudioClip;

    public Image[] playerCards = new Image[numCardsPerLine];
    public Image[] dealerCards = new Image[numCardsPerLine];
    public Image[] splitCards1 = new Image[numCardsPerLine];
    public Image[] splitCards2 = new Image[numCardsPerLine];
    public Image[] splitCards3 = new Image[numCardsPerLine];
    Image[][] cardLine = new Image[numCardLines][];

    int playerNumLives;
    int houseNumLives;
    public deckofcards Deck;
    int[] playerCardPos = new int[numCardLines];                    // next available slot on table to place a card
    int dealerCardPos;
    int currLine;
    int numSplits;                                                  // number of split lines
    int cardLinesDone;                                             // number of card lines the player has finished (stand or busted)
    int cardLinesBusted;
    bool dealersTurnInProgress;
    bool lineCompleted;

    card_t dealerHoleCard;

    int[] numPlayerCards = new int[numCardLines];
    int numDealerCards;
    int[] numPlayerAces = new int[numCardLines];
    int numDealerAces;
    int[] playerTotal = new int[numCardLines];
    int dealerTotal;
    int[] doubleDownBet = new int[numCardLines];
    card_t[] firstCard = new card_t[numCardLines];
    card_t[] secondCard = new card_t[numCardLines];
    bool[] hasFirstCard = new bool[numCardLines];
    bool[] hasSecondCard = new bool[numCardLines];

    bool soundOn = false;

    bool simulating = false;

    // button flares
    public shine dealFlare;
    public shine hitStandFlare;
    public shine doubleDownFlare;
    public shine splitFlare;

    // Update displayed player cash amount.  The
    // actual player cash amount is already set, this
    // just updates the displayed amount.  This is
    // designed to complete is a certain maximum
    // time (cnt * cashTextDelay) independent of the
    // amount being added or subtracted.
    /*
    IEnumerator update_playerWinsText(int num) {
        int i, tmp, cnt;

        cnt = Mathf.Min(Mathf.Abs(num), cashTextIters);
        for (i = 0; i < cnt; i++) {
            tmp = int.Parse(playerNumWinsText.text);
            tmp += (num / cnt);
            playerNumWinsText.text = tmp.ToString();
            yield return new WaitForSeconds(cashTextDelay);
        }

        // make sure it is right when we are done
        playerNumWinsText.text = playerNumWins.ToString();
    }*/

    // see if player or dealer has been dealt blackjack
    //
    // Keep in sync with checkForBJSim()
    bool checkForBJ() {
        if ((playerTotal[currLine] == twenty_one) || ((dealerTotal + dealerHoleCard.value) == twenty_one)) {
            revealHoleCard();
            StartCoroutine(seeWhoWins(true));
            return true;
        }
        else {
            return false;
        }
    }

    // Keep in sync with checkForBJ()
    bool checkForBJSim() {
        if ((playerTotal[currLine] == twenty_one) || ((dealerTotal + dealerHoleCard.value) == twenty_one)) {
            revealHoleCard();
            seeWhoWinsSim(true);                    // use sim version of seeWhoWins()
            return true;
        }
        else {
            return false;
        }
    }

    // We have over 21, see if we have an ace we can change from 11 to 1.
    // Handles special case, player has an ace, has 21, hits and gets another
    // aced, now has 32, demoting an ace still leaves 22, need to demote the other ace
    void checkForAceToLowerPlayer() {
        if (numPlayerAces[currLine] > 0) {
            numPlayerAces[currLine]--;
            playerTotal[currLine] = playerTotal[currLine] - 10;
            playerTotalText[currLine].text = playerTotal[currLine].ToString();
            if (playerTotal[currLine] > twenty_one) {
                checkForAceToLowerPlayer();
            }
        }
    }

    void checkForAceToLowerDealer() {
        if (numDealerAces > 0) {
            numDealerAces--;
            dealerTotal = dealerTotal - 10;
            dealerTotalText.text = dealerTotal.ToString();
            if (dealerTotal > twenty_one) {
                checkForAceToLowerDealer();
            }
        }
    }

    // Player has 'hit' and gone over 21, called to update display
    // as soon as the player busts since we already know this line
    // is a loss, unlike non-busted lines which must wait until
    // after dealer's turn to see who wins.
    void busted() {
        winOrLoseText[currLine].text = "BUSTED !!";
        winOrLoseText[currLine].CrossFadeAlpha(1.0f, 1.0f, false);
        cardLinesBusted++;
        cardLinesDone++;
        playerNumLives--;
        playerNumLivesText.text = playerNumLives.ToString();
        lineCompleted = true;
    }

    // Push....player and dealer have the same amount, give the player
    // his bet back.
    void tie(int line) {
        winOrLoseText[line].text = "PUSH...";
        winOrLoseText[line].CrossFadeAlpha(1.0f, 1.0f, false);

        //StartCoroutine(update_playerWinsText(playerBet + doubleDownBet[line]));
    }

    // dealer's total is greater than player total
    void dealerWins(int line) {
        winOrLoseText[line].text = "YOU LOSE !!";
        winOrLoseText[line].CrossFadeAlpha(1.0f, 1.0f, false);
        playerNumLives--;
        playerNumLivesText.text = playerNumLives.ToString();
    }

    // player had won, return his initial bet and his winnings to him
    void playerWins(bool blackjack, int line) {
        if (blackjack) {
            winOrLoseText[line].text = "BLACKJACK !!";
            winOrLoseText[line].CrossFadeAlpha(1.0f, 1.0f, false);
            houseNumLives -= 2;
            houseNumLivesText.text = houseNumLives.ToString();
        }
        else {
            winOrLoseText[line].text = "YOU WIN !!";
            winOrLoseText[line].CrossFadeAlpha(1.0f, 1.0f, false);
            houseNumLives -= 1 + doubleDownBet[line];
            houseNumLivesText.text = houseNumLives.ToString();
        }
    }

    // All lines have been processed, dealer has taken his turn, for
    // non-busted lines, determine who has won.  Also called when
    // blackjack is hit (blackjack = true).
    //
    // Keep in sync with seeWhoWinsSim()
    IEnumerator seeWhoWins(bool blackjack) {
        disableAll();

        for (int i = 0; i <= currLine; i++) {
            if (playerTotal[i] > twenty_one) {
                // player previsouly busted on this line, it's already been handled
                continue;
            }

            yield return new WaitForSeconds(resultsDelay);

            if (playerTotal[i] > dealerTotal) {
                playerWins(blackjack, i);
            }
            else if (dealerTotal > playerTotal[i]) {
                dealerWins(i);
            }
            else {
                tie(i);
            }
        }

        if ((playerNumLives == 0) || (houseNumLives ==- 0)) {
            globals.playerScore = playerNumLives - houseNumLives;
            yield return new WaitForSeconds(resultsDelay * 2);
            SceneManager.LoadScene("win");
        }

        enableDeal();
    }

    // Keep in sync with seeWhoWins()
    void seeWhoWinsSim(bool blackjack) {
        disableAll();

        for (int i = 0; i <= currLine; i++) {
            if (playerTotal[i] > twenty_one) {
                // player previsouly busted on this line, it's already been handled
                continue;
            }

            if (playerTotal[i] > dealerTotal) {
                playerWins(blackjack, i);
            }
            else if (dealerTotal > playerTotal[i]) {
                dealerWins(i);
            }
            else {
                tie(i);
            }
        }

        if ((playerNumLives == 0) || (houseNumLives == -0)) {
            globals.playerScore = playerNumLives - houseNumLives;
            SceneManager.LoadScene("win");
        }

        enableDeal();
    }

    // If the player has not busted on all lines, dealer
    // plays according to standard blackjack rules, drawing
    // cards until getting 17 or more, or busting.  Dealer
    // hits on soft 17, a total of 17 with an ace which is
    // counted as 11.
    //
    // Keep in sync with doDealersTurnSim()
    IEnumerator doDealersTurn() {
        currLineTagText[currLine].text = "";

        yield return new WaitForSeconds(dealerTurnDelay);
        revealHoleCard();

        if (dealerTotal > twenty_one) {
            // must have been dealt two aces
            checkForAceToLowerDealer();
        }

        // dealer plays if player has non-busted lines
        if (cardLinesDone > cardLinesBusted) {
            // dealer hits on soft 17
            while ((dealerTotal < 17) || ((dealerTotal == 17) && (numDealerAces > 0))) {
                yield return new WaitForSeconds(dealDelay);
                dealCardToDealer();
            }

            if (dealerTotal > twenty_one) {
                // we don't update Text, just internal value
                // set to -1 so playerTotal will be greater then dealerTotal
                dealerTotal = -1;
            }
        }
        
        StartCoroutine(seeWhoWins(false));
    }

    // Keep in sync with doDealersTurn()
    void doDealersTurnSim() {
        revealHoleCard();

        if (dealerTotal > twenty_one) {
            // must have been dealt two aces
            checkForAceToLowerDealer();
        }

        // dealer plays if player has non-busted lines
        if (cardLinesDone > cardLinesBusted) {
            // dealer hits on soft 17
            while ((dealerTotal < 17) || ((dealerTotal == 17) && (numDealerAces > 0))) {
                dealCardToDealer();
            }

            if (dealerTotal > twenty_one) {
                // we don't update Text, just internal value
                // set to -1 so playerTotal will be greater then dealerTotal
                dealerTotal = -1;
            }
        }

        seeWhoWinsSim(false);
    }

    // wrapper for doDealersTurn coroutine
    void dealersTurn() {
        Debug.Log("Dealer's Turn...");
        StartCoroutine(doDealersTurn());
    }

    // Show the hole card that was dealt to the dealer
    // and add its value to the dealer total.
    void revealHoleCard() {
        if (soundOn) {
            audioSource.clip = placeCardAudioClip;
            audioSource.Play();
        }

        dealerTotal += dealerHoleCard.value;
        dealerTotalText.text = dealerTotal.ToString();

        dealerCards[dealerCardPos].sprite = dealerHoleCard.pic;
        dealerCardPos++;

        numDealerCards++;

        if (dealerHoleCard.rank == card_rank.ace) {
            numDealerAces++;
        }
    }

    // Deal next card in the deck to the dealer without
    // showing it, keep it hidden until dealer's turn.
    void dealHoleCardToDealer() {
        dealerHoleCard = Deck.getNextCard();
    }

    // Deal next card to the dealer, updating display
    // and adding card value to dealer total.
    void dealCardToDealer() {
        card_t card;

        if (soundOn) {
            audioSource.clip = placeCardAudioClip;
            audioSource.Play();
        }

        card = Deck.getNextCard();

        dealerTotal += card.value;
        dealerTotalText.text = dealerTotal.ToString();

        dealerCards[dealerCardPos].sprite = card.pic;
        dealerCardPos++;

        numDealerCards++;

        if (card.rank == card_rank.ace) {
            numDealerAces++;
        }

        if (dealerTotal > twenty_one) {
            // must have been dealt two aces
            checkForAceToLowerDealer();
        }
    }

    // Used when splitting cards to remove the split card from
    // the current line after adding it to the next line.
    void removeSecondPlayerCard(card_t card, int line) {
        playerTotal[line] -= card.value;
        playerTotalText[line].text = playerTotal[line].ToString();
        playerCardPos[line]--;
        cardLine[line][playerCardPos[line]].sprite = Resources.Load<Sprite>("card_images/black_card");

        numPlayerCards[line]--;

        if (card.rank == card_rank.ace) {
            // we only call this when splitting doubles, so first card is also an ace
            // return it to its initial value
            playerTotal[line] += 10;
            playerTotalText[line].text = playerTotal[line].ToString();
        }

        hasSecondCard[line] = false;
    }

    // Called when the player is dealt a card and when a card
    // is moved to the next line after splitting.
    void placePlayerCard(card_t card, int line) {
        if (soundOn) {
            audioSource.clip = placeCardAudioClip;
            audioSource.Play();
        }

        playerTotal[line] += card.value;
        playerTotalText[line].text = playerTotal[line].ToString();

        cardLine[line][playerCardPos[line]].sprite = card.pic;
        playerCardPos[line]++;

        numPlayerCards[line]++;

        if (card.rank == card_rank.ace) {
            numPlayerAces[line]++;
        }

        // save rank of first and second card dealt to player to check for split opportunity
        if (!hasFirstCard[line]) {
            firstCard[line] = card;
            hasFirstCard[line] = true;
        }
        else if (!hasSecondCard[line]) {
            secondCard[line] = card;
            hasSecondCard[line] = true;
        }
    }

    // get next card from the deck and deal it to the player
    void dealCardToPlayer() {
        card_t card;

        card = Deck.getNextCard();
        placePlayerCard(card, currLine);

        if (playerTotal[currLine] > twenty_one) {
            // must have been dealt two aces
            checkForAceToLowerPlayer();
        }
    }


    // Deal two cards to player and to dealer, subtract bet amount
    // and enable apporpriate buttons.
    //
    // Keep in sync with doDealSim()
    IEnumerator doDeal() {
        if (soundOn) {
            audioSource.clip = shuffleAudioClip;
            audioSource.Play();
        }

        yield return new WaitForSeconds(dealDelay);

        Deck.shuffleDeck();

        yield return new WaitForSeconds(dealDelay);
        currLineTagText[currLine].text = "@";
        dealCardToPlayer();

        yield return new WaitForSeconds(dealDelay);
        dealCardToDealer();

        yield return new WaitForSeconds(dealDelay);
        dealCardToPlayer();

        yield return new WaitForSeconds(dealDelay);
        dealHoleCardToDealer();

        // anyone get dealt blackjack?
        if (!checkForBJ()) {
            enableHitStand();

            if (firstCard[currLine].rank == secondCard[currLine].rank) {
                enableSplit();
            }

            if ((playerTotal[currLine] == 9) || (playerTotal[currLine] == 10) || (playerTotal[currLine] == 11)) {
                enableDoubleDown();
            }
        }
    }

    // Keep in sync with doDeal()
    //
    // Returns true if BJ was dealt
    bool doDealSim() {
        Deck.shuffleDeck();
        dealCardToPlayer();
        dealCardToDealer();
        dealCardToPlayer();
        dealHoleCardToDealer();

        // anyone get dealt blackjack?
        if (!checkForBJSim()) {                                   // using sim version of checkForBJ()
            enableHitStand();

            if ((playerTotal[currLine] == 9) || (playerTotal[currLine] == 10) || (playerTotal[currLine] == 11)) {
                enableDoubleDown();
            }
            return false;
        } else {
            return true;
        }
    }

    // player clicked 'DEAL', start a new round of blackjack
    public void Deal() {
        disableAll();
        init();

        StartCoroutine(doDeal());
    }

    // player clicked 'STAND', mark this line as complete
    //
    // simHands() will call this directly, don't change to coroutine
    public void Stand() {
        disableAll();
        cardLinesDone++;
        lineCompleted = true;
    }

    // player clicked 'HIT' , deal him another card
    //
    // Keep in sync with HitSim()
    public void Hit() {
        disableDoubleDown();
        disableSplit();

        dealCardToPlayer();

        if (playerTotal[currLine] > twenty_one) {
            disableHitStand();
            busted();
        }
    }

    // returns false to sim if player busts, true otherwise
    //
    // keep in sync with Hit()
    public bool HitSim() {
        disableDoubleDown();
        disableSplit();

        dealCardToPlayer();

        if (playerTotal[currLine] > twenty_one) {
            disableHitStand();
            busted();
            return false;
        } else {
            return true;
        }
    }

    // Player clicked 'DOUBLE DOWN', deal him one card
    // and mark this line complete.
    public void doubleDown() {
        disableAll();
        doubledDownText[currLine].text = "x2";
        doubleDownBet[currLine] = 1;

        dealCardToPlayer();
        cardLinesDone++;
        lineCompleted = true;
    }

    // Finish a line that was started by the player splitting,
    // deal second card to player and enable appropriate buttons.
    void handleSplits() {
        currLineTagText[currLine].text = "";
        currLine++;
        currLineTagText[currLine].text = "@";

        dealCardToPlayer();

        if ((playerTotal[currLine] == 9) || (playerTotal[currLine] == 10) || (playerTotal[currLine] == 11)) {
            enableDoubleDown();
        }

        if (firstCard[currLine].rank == secondCard[currLine].rank) {
            enableSplit();
        }

        enableHitStand();
    }

    // Split the players line, moving the second card from
    // this line to the first slot of the next available line,
    // deal another card to the current line, and enable
    // appropriate buttons.
    IEnumerator doSplit() {
        numSplits++;

        placePlayerCard(secondCard[currLine], numSplits);
        removeSecondPlayerCard(secondCard[currLine], currLine);

        yield return new WaitForSeconds(dealDelay);
        dealCardToPlayer();

        enableHitStand();

        if ((playerTotal[currLine] == 9) || (playerTotal[currLine] == 10) || (playerTotal[currLine] == 11)) {
            enableDoubleDown();
        }

        if (firstCard[currLine].rank == secondCard[currLine].rank) {
            enableSplit();
        }
    }

    // player clicked "SPLIT"
    public void split() {
        disableAll();

        StartCoroutine(doSplit());
    }

    public void setSFX(bool sfx) {
        soundOn = sfx;
    }

    // reset things for a new deal
    void init() {
        for (int i = 0; i < numCardLines; i++) {
            playerTotal[i]              = 0;
            playerTotalText[i].text     = "";
            hasFirstCard[i]             = false;
            hasSecondCard[i]            = false;
            playerCardPos[i]            = 0;
            numPlayerCards[i]           = 0;
            numPlayerAces[i]            = 0;
            doubleDownBet[i]            = 0;
            doubledDownText[i].text     = "";
            currLineTagText[i].text     = "";
        }
        currLine            = 0;
        numSplits           = 0;
        cardLinesDone       = 0;
        cardLinesBusted     = 0;

        dealerCardPos           = 0;
        numDealerCards          = 0;
        numDealerAces           = 0;
        dealerTotal             = 0;
        dealerTotalText.text    = "";
        dealersTurnInProgress   = false;
        lineCompleted           = false;

        for (int i = 0; i < numCardLines; i++) {
            winOrLoseText[i].CrossFadeAlpha(0.0f, 0.5f, false);
        }

        foreach (Image I in playerCards) {
            I.sprite = Resources.Load<Sprite>("card_images/black_card");
        }
        foreach (Image I in splitCards1) {
            I.sprite = Resources.Load<Sprite>("card_images/black_card");
        }
        foreach (Image I in splitCards2) {
            I.sprite = Resources.Load<Sprite>("card_images/black_card");
        }
        foreach (Image I in splitCards3) {
            I.sprite = Resources.Load<Sprite>("card_images/black_card");
        }
        foreach (Image I in dealerCards) {
            I.sprite = Resources.Load<Sprite>("card_images/black_card");
        }
    }

    // Use this for initialization
    void Start() {
        Deck = new deckofcards();

        levelText.text = globals.currentLevel.ToString();

        enableDeal();
        disableHitStand();
        disableDoubleDown();
        disableSplit();
        init();

        playerNumLives = globals.numLivesStart;
        playerNumLivesText.text = playerNumLives.ToString();
        houseNumLives = globals.numLivesStart;
        houseNumLivesText.text = houseNumLives.ToString();

        cardLine[0] = playerCards;
        cardLine[1] = splitCards1;
        cardLine[2] = splitCards2;
        cardLine[3] = splitCards3;
    }

    void Update() {
        if (simulating) {
            // we are in simulation mode, dealer's turn will get triggered
            // by sim code
            return;
        }

        if ((cardLinesDone == numSplits + 1) && (!dealersTurnInProgress)) {
            dealersTurnInProgress = true;
            dealersTurn();
        }

        if (lineCompleted) {
            // completed a line, are there more to be processed?
            lineCompleted = false;
            if (cardLinesDone < numSplits + 1) {
                handleSplits();
            }
        }
    }

    // enable / disable UI buttons

    void enableDeal() {
        dealFlare.enableBurst();
        dealButtonText.gameObject.SetActive(true);
    }

    void disableDeal() {
        dealFlare.cancelBurst();
        dealButtonText.gameObject.SetActive(false);
    }

    void enableHitStand() {
        hitStandFlare.enableBurst();
        hitButtonText.gameObject.SetActive(true);
        standButtonText.gameObject.SetActive(true);
    }

    void disableHitStand() {
        hitStandFlare.cancelBurst();
        hitButtonText.gameObject.SetActive(false);
        standButtonText.gameObject.SetActive(false);
    }

    void enableDoubleDown() {
        doubleDownFlare.enableBurst();
        doubleDownButtonText.gameObject.SetActive(true);
    }

    void disableDoubleDown() {
        doubleDownFlare.cancelBurst();
        doubleDownButtonText.gameObject.SetActive(false);
    }

    void enableSplit() {
        splitFlare.enableBurst();
        splitButtonText.gameObject.SetActive(true);
    }

    void disableSplit() {
        splitFlare.cancelBurst();
        splitButtonText.gameObject.SetActive(false);
    }

    void disableAll() {
        disableDeal();
        disableHitStand();
        disableDoubleDown();
        disableSplit();
    }

    IEnumerator simHands() {
        while (true) {
            bool simplayerStandsOrBusted;

            Debug.Log("Dealing...");
            init();                                     // from Deal()
            if (!doDealSim()) {
                // BJ was not dealt

                simplayerStandsOrBusted = false;
                while (!simplayerStandsOrBusted) {
                    Debug.Log("player(" + playerTotal[0] + ") dealer(" + dealerTotal + ")");

                    if (playerTotal[0] < 12) {
                        Debug.Log("Hitting...");
                        // can't bust here
                        HitSim();
                    } else if ((dealerTotal > 6) && (playerTotal[0] < 17)) {
                        Debug.Log("Hitting...");
                        if (!HitSim())
                        {
                            // busted
                            Debug.Log("Busted");
                            simplayerStandsOrBusted = true;
                        }
                    }
                    else if ((playerTotal[0] == 12) && (dealerTotal < 4)) {
                        Debug.Log("Hitting...");
                        if (!HitSim()) {
                            // busted
                            Debug.Log("Busted");
                            simplayerStandsOrBusted = true;
                        }
                    } else {
                        Debug.Log("Standing...");
                        Stand();
                        simplayerStandsOrBusted = true;
                    }
                }

                Debug.Log("Dealer's Turn...");
                doDealersTurnSim();
            }

            yield return new WaitForSeconds(3.0f);
        }
    }

    public void simulate() {
        const int simLifeMult = 50;

        Debug.Log("Simulating...");

        simulating = true;
        soundOn = false;

        // multiply number of lives when simulationg so we get longer, more statisdtical reults
        playerNumLives *= simLifeMult;
        playerNumLivesText.text = playerNumLives.ToString();
        houseNumLives *= simLifeMult;
        houseNumLivesText.text = houseNumLives.ToString();

        StartCoroutine(simHands());
    }
}

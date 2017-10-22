using System;
using System.Collections.Generic;
using System.Text;
using BitPoker.Models.Deck;
using BitPoker.Models.Hands;
using System.Collections.ObjectModel;

namespace PokerRules.Games
{
    /// <summary>
    /// The base class of poker games. Derive from this class to implement a new game flavor.
    /// </summary>
    public abstract class BaseGame : IRulesInterpreter
    {
        // The deck used in this game
        private BitPoker.Models.Deck.Deck deck = new BitPoker.Models.Deck.Deck();

        // An array of cards for each player
        private List<Card>[] playerCards = new List<Card>[0];

        // An array which indicates if a player has folded
        private bool[] foldedPlayers = new bool[0];

        // The defined game hand families
        private SortedList<int, HandFamily> families = new SortedList<int, HandFamily>();

        /// <summary>
        /// Creates a new instance of the BaseGame class
        /// </summary>
        protected BaseGame()
        {
        }

        /// <summary>
        /// Gets the maximal player count
        /// </summary>
        public abstract int MaximalPlayersLimit { get; }


        /// <summary>
        /// Starts a new game with the specified amount of players.
        /// </summary>
        /// <param name="numberOfPlayers">
        /// The number of starting players. Must be larger than 1.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">If the number of players is less than 1 or higher 
        /// than <see cref="MaximalPlayersLimit"/></exception>
        public void BeginGame(int numberOfPlayers)
        {
            if (numberOfPlayers < 1)
                throw new ArgumentOutOfRangeException("numberOfPlayers", "Must have at least one player");
            if (numberOfPlayers > MaximalPlayersLimit)
                throw new ArgumentOutOfRangeException("numberOfPlayers", "Can't add players over the maximal player limit");
            
            // shuffle the deck of cards.
            deck.Shuffle(numberOfPlayers * 2);
            // prepare the player card holders
            playerCards = new List<Card>[numberOfPlayers];
            // prepare the player fold state
            foldedPlayers = new bool[numberOfPlayers];

            for (int i = 0; i < playerCards.Length; ++i)
            {
                playerCards[i] = new List<Card>();
                foldedPlayers[i] = false;
            }
            families.Clear();
            // Get the defined order of hand families for this game and store it according to their family value
            foreach (HandFamily family in GetFamilies())
            {
                if (families.ContainsKey(family.FamilyValue))
                {
                    throw new InvalidOperationException("Derived class must assign unique values to the families returned by GetFamilies()");
                }
                families.Add(family.FamilyValue, family);
            }
            // start the game
            OnBeginGame();
        }

        /// <summary>
        /// Gets the number of players which started the game
        /// </summary>
        public int NumberOfPlayers { get { return playerCards.Length; } }

        /// <summary>
        /// Called when the game starts, after all of the data structures were initialized
        /// </summary>
        protected abstract void OnBeginGame();

        /// <summary>
        /// Gets the current game deck.
        /// </summary>
        protected BitPoker.Models.Deck.Deck Deck { get { return deck; } }

        /// <summary>
        /// Gets the game defined hand families. Override this to define a custom hand order.
        /// </summary>
        /// <returns>
        /// A read only collection of hand families which defines the hand order.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The returned value must not be null and each family must have a unique <see cref="HandFamily.FamilyValue"/>.
        /// </para>
        /// <para>
        /// When overriding this method, a game can define a new custom order of hands families. The pre-defined order is as follows:
        /// <list type="ordered">
        /// <item>High Card</item>
        /// <item>Two of a Kind</item>
        /// <item>Two Pair</item>
        /// <item>Three of a Kind</item>
        /// <item>Straight</item>
        /// <item>Flush</item>
        /// <item>Full House</item>
        /// <item>Four of a Kind</item>
        /// <item>Straight Flush</item>
        /// </list>
        /// </para>
        /// </remarks>
        public virtual ReadOnlyCollection<HandFamily> GetFamilies()
        {
            HandFamily[] result = new HandFamily[9];
            result[0] = new HighCardFamily(0);
            result[1] = new TwoOfAKindFamily(1);
            result[2] = new TwoPairFamily(2);
            result[3] = new ThreeOfAKindFamily(3);
            result[4] = new StraightFamily(4);
            result[5] = new FlushFamily(5);
            result[6] = new FullHouseFamily(6);
            result[7] = new FourOfAKindFamily(7);
            result[8] = new StraightFlushFamily(8);
            return Array.AsReadOnly<HandFamily>(result);
        }

        /// <summary>
        /// Deals the given card to the specified player. 
        /// </summary>
        /// <param name="player">The player to deal to</param>
        /// <param name="card">The card value to add to the player</param>
        /// <exception cref="InvalidOperationException">Is thrown if the given player has folded</exception>
        /// <exception cref="IndexOutOfRangeException">Is thrown if the given player is out of the range [0-<see cref="NumberOfPlayers"/>)</exception>
        protected void DealToPlayer(int player, Card card)
        {
            if (foldedPlayers[player])
                throw new InvalidOperationException("player has folded");

            playerCards[player].Add(card);
        }

        /// <summary>
        /// Deals the same card to all of the players.
        /// </summary>
        /// <param name="card">The card which is added to all of the players.</param>
        /// <remarks>
        /// This method is useful to games which have community cards. Use it to deal the community cards to all of the players.
        /// </remarks>
        protected void DealToAll(Card card)
        {
            for (int i = 0; i < playerCards.Length; ++i)
            {
                // go over all of the playing players and add the card to their list
                if (!foldedPlayers[i])
                    playerCards[i].Add(card);
            }
        }

        /// <summary>
        /// Deals all of the player the next cards in the deck according to their order.
        /// </summary>
        /// <param name="cardNumber">The number of cards to add to each player</param>
        /// <remarks>
        /// Every player is dealt a unique card out of the top of the deck. 
        /// </remarks>
        /// <exception cref="InvalidOperationException">Is thrown if the deck runs out of cards.</exception>
        protected void DealToAll(int cardNumber)
        {
            // add the defined number of cards.
            // this loop is external to prevent the players getting the cards out of the deck sequentialy
            for (int i = 0; i < cardNumber; ++i)
            {
                for (int player = 0; player < NumberOfPlayers; ++player)
                {
                    // go over all of the playing players and add them a new card out of the top of the deck
                    if (!foldedPlayers[player])
                    {
                        DealToPlayer(player, Deck.Deal());
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the winnig players and adds their winning hands to the given list of hands
        /// </summary>
        /// <param name="resultHands">
        /// An empty list of hands.
        /// </param>
        /// <returns>
        /// An array of player indecies who won the game.
        /// </returns>
        private int[] CalculatePlayersWithWinningHands(List<Hand> resultHands)
        {
            // The highest hand.
            Hand winningHand = null;
            // The list of winners.
            List<int> playersWithHighest = new List<int>();

            // loop over all of the playing players
            for (int player = 0; player < playerCards.Length; ++player)
            {
                if (!foldedPlayers[player])
                {
                    // flag to replace winning hand
                    bool replaceWinningHand = false;
                    List<Card> curCards = playerCards[player];
                    // Get the player best hand
                    Hand playerBestHand = GetBestHand(curCards);

                    if (winningHand == null) // replace the hand if there is no winning hand
                    {
                        replaceWinningHand = true;

                    }
                    else if (playerBestHand != null) 
                    {
                        int compareResult = playerBestHand.CompareTo(winningHand);
                        if (compareResult > 0) // replace the hand if the current hand is better then the winning hand
                        {
                            replaceWinningHand = true;
                        }
                        else if (compareResult == 0) // add the current player to the winners list if it has the same hand value.
                        {
                            playersWithHighest.Add(player);
                            resultHands.Add(playerBestHand);
                        }
                    }

                    // found a new better high hand:
                    if (replaceWinningHand) 
                    {
                        winningHand = playerBestHand; // replace the hand

                        playersWithHighest.Clear(); // clear the old winners
                        resultHands.Clear(); // clear the old high hands

                        playersWithHighest.Add(player); // add the new winner
                        resultHands.Add(playerBestHand); // and the new best hand
                    }
                }
            }
            return playersWithHighest.ToArray();
        }

        /// <summary>
        /// Get the player best hand using all of the given cards.
        /// </summary>
        /// <param name="curCards">A list of cards to search in for the best hand. Must not be null</param>
        /// <returns>
        /// The best hand which could be found in the given cards, or null if none found.
        /// </returns>
        /// <remarks>
        /// The hand families order was defined in <see cref="GetFamilies"/>
        /// </remarks>
        private Hand getBestHand(List<Card> curCards)
        {
            // go over all of the hand families, from highest value to lowest
            for (int i = families.Values.Count - 1; i >= 0; --i)
            {
                // try getting the hand using the current hand family
                Hand curHand = families.Values[i].GetBestHand(curCards);
                if (curHand != null) // if found a hand, it will have the highest value, return it.
                    return curHand;
            }
            // no hand was found.
            return null;
        }

        /// <summary>
        /// Replace the given player old card with the new card
        /// </summary>
        /// <param name="player">The player of whom to replace the cards. Must be in the range [0-<see cref="NumberOfPlayers"/>)</param>
        /// <param name="oldCard">The old card to drop. The player must have it</param>
        /// <param name="newCard">The new card to add.</param>
        /// <exception cref="IndexOutOfRangeException">Is thrown if the given player is out of the range [0-<see cref="NumberOfPlayers"/>)</exception>
        /// <exception cref="InvalidOperationException">Is thrown if the old card did not exist in the player hand</exception>
        /// <remarks>
        /// Use this method to draw cards on behalf of the player.
        /// </remarks>
        protected void ReplaceCard(int player, Card oldCard, Card newCard)
        {
            if (playerCards[player].Remove(oldCard))
            {
                playerCards[player].Add(newCard);
            }
            else throw new InvalidOperationException("player must have the old card to replace");

        }

        /// <summary>
        /// Gets a read only collection of the player cards.
        /// </summary>
        /// <param name="player">The player of whom to get the cards</param>
        /// <returns>
        /// A read only collection containing the player cards.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">Is thrown if the given player is out of the range [0-<see cref="NumberOfPlayers"/>)</exception>
        public ReadOnlyCollection<Card> GetPlayerCards(int player)
        {
            return playerCards[player].AsReadOnly();
        }

        /// <summary>
        /// Get the given player best hand, or null if none exists.
        /// </summary>
        /// <param name="player">The player of whom to get the best hand.</param>
        /// <returns>
        /// The player best hand or null if the player has folded or doesn't have a best hand.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">Is thrown if the given player is out of the range [0-<see cref="NumberOfPlayers"/>)</exception>
        public Hand GetPlayerBestHand(int player)
        {
            if (foldedPlayers[player])
                return null;
            return GetBestHand(playerCards[player]);
        }

        /// <summary>
        /// Gets the best hand according to this game logic out of the given cards.
        /// </summary>
        /// <param name="cards">A collection of cards to search for the best hand. Must not be null.</param>
        /// <returns>
        /// The best hand which can be creates using the given cards or null if none exists.
        /// </returns>
        /// <remarks>
        /// The hand families order was defined in <see cref="GetFamilies"/>
        /// </remarks>
        public Hand GetBestHand(IEnumerable<Card> cards)
        {
            // use a clone to pass to the derived classes
            List<Card> clone = new List<Card>(cards);

            return GetBestHandOverride(clone);
        }

        /// <summary>
        /// Gets the best hand according to this game logic out of the given cards.
        /// </summary>
        /// <param name="clone">A list of cards to search for the best hand, not null and can be modified.</param>
        /// <returns>
        /// The best hand which can be creates using the given cards or null if none exists.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The hand families order was defined in <see cref="GetFamilies"/>
        /// </para>
        /// <para>
        /// Override this method to define a new logic of how to create a player hand. When doing so, you can use the base implementation
        /// or use the families directly defined by <see cref="GetFamilies"/>
        /// </para>
        /// </remarks>
        protected virtual Hand GetBestHandOverride(List<Card> clone)
        {
            return getBestHand(clone);
        }

        /// <summary>
        /// Ends the game and returns an array of <see cref="GameResult"/> to declare the winner(s)
        /// </summary>
        /// <returns>
        /// An array which holds the winning results.
        /// </returns>
        public GameResult[] EndGame()
        {
            // notify the end of the game to derived classes.
            OnEndGame();
            // the empty list of which will be filled with winning hands
            List<Hand> winningHands = new List<Hand>();
            // calculate the winners
            int[] players = CalculatePlayersWithWinningHands(winningHands);
            // generate new results according to the winners
            GameResult[] result = new GameResult[players.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = new GameResult(players[i], winningHands[i]);
            }
            return result;
        }

        /// <summary>
        /// Folds the given player.
        /// </summary>
        /// <param name="player">The player to fold</param>
        /// <exception cref="IndexOutOfRangeException">Is thrown if the given player is out of the range [0-<see cref="NumberOfPlayers"/>)</exception>
        public void FoldPlayer(int player)
        {
            foldedPlayers[player] = true;
            // notify the folding player to derived classes
            OnPlayerFolds(player);
        }
        
        /// <summary>
        /// Determines if the given player has folded
        /// </summary>
        /// <param name="player">
        /// The player to check if has folded
        /// </param>
        /// <returns>
        /// True if the player folds.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">Is thrown if the given player is out of the range [0-<see cref="NumberOfPlayers"/>)</exception>
        protected bool IsPlayerFolder(int player)
        {
            return foldedPlayers[player];
        }
        /// <summary>
        /// Called when the player folds. Derive it to add special logic when player folds/
        /// </summary>
        /// <param name="player">
        /// The index of the folding player
        /// </param>
        protected virtual void OnPlayerFolds(int player)
        {
        }

        /// <summary>
        /// Called when the game ends, implement the end game logic here.
        /// </summary>
        protected abstract void OnEndGame();
    }
}

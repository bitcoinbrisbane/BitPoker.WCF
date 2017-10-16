using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using PokerEngine;
using PokerEngine.Engine;
using BitPoker.Models.Deck;

namespace PokerService
{
    /// <summary>
    /// The interface which describes clients callback contract.
    /// </summary>
    public interface IPokerClient
    {
        /// <summary>
        /// Called when the betting round finalizes in any case the round is over.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void FinalizeBettingRound();

        /// <summary>
        /// Called when the betting round is completed and there is a single winner.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyAllFolded();

        /// <summary>
        /// Called when a betting round is completed and all players have checked.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyBetRoundComplete();

        /// <summary>
        /// Called by the client when the blind open is made.
        /// </summary>
        /// <param name="opened">The player which blind opens</param>
        /// <param name="openAmount">The player open amount, may be 0</param>
        [OperationContract(IsOneWay = true)]
        void NotifyBlindOpen(Player opened, int openAmount);

        /// <summary>
        /// Called by the client when the blind raise is made.
        /// </summary>
        /// <param name="raiser">The player which blind raises</param>
        /// <param name="raiseAmount">The raise amound, may be 0</param>
        [OperationContract(IsOneWay = true)]
        void NotifyBlindRaise(Player raiser, int raiseAmount);

        /// <summary>
        /// Called by the client to notify the current hand which is played
        /// </summary>
        /// <param name="currentHand">The current hand</param>
        [OperationContract(IsOneWay = true)]
        void NotifyCurrentHand(int currentHand);

        /// <summary>
        /// Called by the client when a round starts.
        /// </summary>
        /// <param name="dealer">The current round dealer</param>
        /// <param name="potAmount">The starting amount of money in the pot</param>
        [OperationContract(IsOneWay = true)]
        void NotifyDealerAndPotAmount(Player dealer, int potAmount);

        /// <summary>
        /// Called by the client when a game is canceled by the server
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyGameCanceled();

        /// <summary>
        /// Called by the client when a game is already in progress. The server can't be connected
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyGameInProgress();

        /// <summary>
        /// Called by the client when the name passed by <see cref="IPokerService.Login"/> already exists on the server.
        /// </summary>
        /// <remarks>
        /// Derived classes may use this method to create a new login name.
        /// </remarks>
        [OperationContract(IsOneWay = true)]
        void NotifyNameExists();
        
        /// <summary>
        /// Called by the client when a new player is connected.
        /// </summary>
        /// <param name="player">The new player which was connected</param>
        [OperationContract(IsOneWay = true)]
        void NotifyNewUserConnected(Player player);

        /// <summary>
        /// Called by the client when a player disconnects
        /// </summary>
        /// <param name="player">The player which was disconnected</param>
        [OperationContract(IsOneWay = true)]
        void NotifyUserDisconnected(Player player);

        /// <summary>
        /// Called by the client when a player performs an action.
        /// </summary>
        /// <param name="player">The player which performed the action</param>
        /// <param name="betAction">The action performed</param>
        /// <param name="callAmount">The call amount if any, can be 0</param>
        /// <param name="raiseAmount">The raise amount if any, can be 0</param>
        [OperationContract(IsOneWay = true)]
        void NotifyPlayerAction(Player player, BetAction betAction, int callAmount, int raiseAmount);

        /// <summary>
        /// Called when a player is thinking of a move
        /// </summary>
        /// <param name="thinkingPlayer">The player which is thinking</param>
        [OperationContract(IsOneWay = true)]
        void NotifyPlayerIsThinking(Player thinkingPlayer);

        /// <summary>
        /// Called by the clien when a player wins/loses.
        /// </summary>
        /// <param name="player">The player which won/lost</param>
        /// <param name="isWinner">True - the player won. False - The player lost.
        /// </param>
        [OperationContract(IsOneWay = true)]
        void NotifyPlayerStatus(Player player, bool isWinner);

        /// <summary>
        /// Called by the client after all of the results arrived
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyResultsIncomingCompleted();


        /// <summary>
        /// Called by the client when the current client can't participate in the current round.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifyStartingGame"/> is called prior to the round start.
        /// </remarks>
        [OperationContract(IsOneWay = true)]
        void NotifySittingOut();

        /// <summary>
        /// Called by the client when the current round starts and the client is participating.
        /// </summary>
        /// <remarks>
        /// Either this method or <see cref="NotifySittingOut"/> is called prior to the round start.
        /// </remarks>
        [OperationContract(IsOneWay = true)]
        void NotifyStartingGame();

        /// <summary>
        /// Called by the client when each round ends. 
        /// </summary>
        /// <param name="gameOver">A flag which indicates the game is over.</param>
        /// <param name="winner">The winning player (if any), may be null</param>
        [OperationContract(IsOneWay = true)]
        void WaitOnRoundEnd(bool gameOver, Player winner);

        /// <summary>
        /// Called by the client to get the current player betting action. Derived classes must implement it and respond with a 
        /// proper action
        /// </summary>
        /// <param name="player">The player which needs to bet</param>
        /// <param name="action">The action to use to pass the player response</param>
        /// <returns>
        /// The player action modified to reflect the player action taken.
        /// </returns>
        [OperationContract()]
        PlayerBettingAction WaitPlayerBettingAction(Player player, PlayerBettingAction action);

        /// <summary>
        /// Called by the client when an update message arrives.
        /// </summary>
        /// <param name="player">The players sorted by their round order</param>
        /// <param name="potInformation">The current state of the pot</param>
        /// <param name="communityCards">The game community cards, if any. Can be null</param>
        [OperationContract(IsOneWay = true)]
        void WaitSynchronization(IEnumerable<Player> player, PotInformation potInformation, Card[] communityCards);

        /// <summary>
        /// A method which is used to get the player drawing action. Derived classes must override this method to perform the action.
        /// </summary>
        /// <param name="player">The player which may draw new cards</param>
        /// <param name="action">The drawing action which must be updated with the player drawing selection</param>
        [OperationContract()]
        PlayerDrawingAction WaitPlayerDrawingAction(Player player, PlayerDrawingAction action);

        /// <summary>
        /// Called by the engine to notify the player with the new cards
        /// </summary>
        /// <param name="curPlayer">The player with the new drawn cards</param>
        [OperationContract(IsOneWay = true)]
        void NotifyPlayerNewCards(Player curPlayer);

        /// <summary>
        /// Called by the client to notify how many cards were drawn by the player
        /// </summary>
        /// <param name="player">The drawing players</param>
        /// <param name="drawCount">The amount of cards drawn. Can be 0</param>
        [OperationContract(IsOneWay = true)]
        void NotifyPlayerDraws(Player player, int drawCount);

        /// <summary>
        /// Called by the engine after the drawing round completes.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void NotifyDrawComplete();

        /// <summary>
        /// Called by the server to notify clients of server shutdown
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Bye();
    }
}

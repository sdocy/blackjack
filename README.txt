
https://sdocy.github.io/blackjack/

Try it out at

https://sdocy.itch.io/


A full featured BlackJack game that started as a chapter exercise on learncpp.com. I was amazed at how
simple it was to capture the most basic rules of BlackJack, so I decided to implement a graphical version
in Unity. My C++ code transferred over to Unity and C# very easily and I implemented the rules that had not
been part of the exercise on learncpp.com, getting blackjack with two cards, and aces worth either 1 or 11.

Adding doubling down was very straight-forward, but adding splitting was quite tricky, as I knew it would
be. Instead of dealing with a single hand, I had to account for multiple hands. Even though I had accounted
for this and allowed for my implementation to be changed to an array-based implementation, it required some
refactoring of my code to get an elegant design. And of course I had to make sure that doubling-down after
splitting worked correctly

Perhaps the trickiest part was figuring out how and where to add appropriate delays for actions like
displaying dealt cards on the screen. Being able to follow the action as it unfolds instead of immediately
racing to the final result made the game feel much more natural and increased the suspense of whether you
were going to win or not. I had used InvokeRepeating() before but not StartCoroutinue(), which is what this
really called for, in able to be able to use yield to introduce delays at specific points in the game.
Getting the proper mix of event-driven code and asynchronous delays was tricky, but it all came together
eventually.

My plans are to eventually combine this with some sort of story-driven trading card game. Cards would
impact the standard BlackJack rules, such as increasing or decreasing the total at which a player busts,
switching cards with the other player, etc.  To facilitate testing how much certain rule variations would
impact a player's chances of winning, I implemented a simulation mode in which the computer would basically
play itself, using commonly used rules for when to hit and when to stand. The simulation allows me to run
though hundreds of hands quickly and automatically. Implementing the simulator was especially tricky
because I had to remove the asynchronous nature of the code which contained delays. I ended up having to
duplicate certain functions which triggered asynchronous code and create synchronous versions of them. The
functions are all fairly simple, so it's not as bad as it sounds, but I will probably take a look at it
again at some point and see if I can come up with a better solution.
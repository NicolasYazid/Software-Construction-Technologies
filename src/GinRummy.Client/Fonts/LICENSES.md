# Fonts embedded in the client

The three files in this folder are compiled into the assembly as `Resource`, so the
client renders the same on any machine without installing anything. The three
typefaces come from different authors and do not share the same terms, so each one
is recorded here with the terms it was obtained under.

## Cairopixel — `Cairopixel-Medium.ttf`

- Author: GGBotNet, https://ggbot.net
- Licence: SIL Open Font License 1.1, full text in `OFL-Cairopixel.txt`
- Embedding permissions declared by the file: unrestricted (`fsType` 0)
- Coverage: 900 glyphs, the whole Latin alphabet with accents included

It is the body family and the only one of the three that covers the accented
vowels, the enye and the inverted marks, so the other two name it as the next
family in their list. Its em measures 16 design pixels, which makes it render
without softening at sizes that are multiples of 16. Its capitals measure 0.44 of
the em, well under the 0.78 of the family it replaces, so every size that uses it
carries a higher number for the same height on screen.

## RetroByte — `RetroByte-Medium.ttf`

- Author: Igor Ovsyannykov
- Licence: the file declares none
- Embedding permissions declared by the file: unrestricted (`fsType` 0)
- Coverage: 118 glyphs, ASCII only

It is the display family. It carries **no accented vowel, no enye and no opening
question mark**, so the Spanish labels of the menu are drawn by two families at
once: RetroByte for the letters it has and Cairopixel for the rest. The difference
is visible, because the capitals of RetroByte measure 0.56 of the em against the
0.44 of Cairopixel, and the borrowed letter therefore sits lower and smaller than
the ones beside it. In Spanish this affects `INICIAR SESIÓN`, `CÓMO JUGAR` and the
`ESPAÑOL` of the language selector.

## HigherPixels — `HigherPixels-Regular.ttf`

- Author: Max Infeld, Xerographer Fonts, xerographer.blogspot.com
- Licence: all rights reserved, contact maxinfeld@gmail.com
- Embedding permissions declared by the file: editable embedding (`fsType` 8)
- Coverage: letters and digits, 70 mapped code points, no accents at all

It is used only by the wordmark, which reads `GIN RUMMY`, so the letters it lacks
never reach the screen through it.

## Consequence for the project

Only Cairopixel satisfies STK-13 on its own. `FntDisplay` and `FntBrand` are
declared as family lists with Cairopixel behind them, which keeps every missing
glyph inside a pixel typeface instead of dropping it to a font of the system, but
it does not make the two families match.

The circle the password field draws by default, `U+25CF`, is in none of the three.
`StyFieldPassword` therefore states `PasswordChar` as `U+2022`, which Cairopixel
carries, instead of letting the control ask for a character the font does not have.

Cairopixel ships a single weight, so every style that asked for a bold face now
states `Normal`. The weight the framework synthesises thickens the strokes by a
fraction of a pixel, which blurs the square edges these typefaces are chosen for.

`OFL-Cairopixel.txt` must travel with its font: the licence requires each copy to
carry its copyright notice and its terms, so removing it from the repository would
break the condition under which Cairopixel may be redistributed.

Neither RetroByte nor HigherPixels grants redistribution in writing, and RetroByte
carries no licence statement at all. The project is academic and produces no
income, but if the course requires every asset to carry explicit redistribution
rights both should be replaced. Cairopixel itself, Silkscreen, Press Start 2P and
VT323 are pixel families under the Open Font License.

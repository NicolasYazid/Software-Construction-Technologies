# Fonts embedded in the client

The four files in this folder are compiled into the assembly as `Resource`, so the
client renders the same on any machine without installing anything. The three
typefaces come from different authors and do not share the same terms, so each one
is recorded here with the terms it was obtained under.

## Pixeloid Sans — `PixeloidSans-Regular.ttf`, `PixeloidSans-Bold.ttf`

- Author: GGBotNet, https://ggbot.net/fonts/
- Licence: SIL Open Font License 1.1, full text in `OFL-PixeloidSans.txt`
- Embedding permissions declared by the file: unrestricted (`fsType` 0)

The licence allows embedding and redistribution, including inside this repository,
as long as the copyright notice and the licence travel with the font. That is why
`OFL-PixeloidSans.txt` is part of the project and must not be removed.

This is the only family of the three that covers the whole Latin alphabet with
accents, so it is the one that resolves the characters the other two lack.

## Pixelta — `Pixelta-Regular.ttf`

- Author: Blankids Studio, https://www.blankidsfonts.com
- Licence: personal use only, commercial use not granted
- Embedding permissions declared by the file: unrestricted (`fsType` 0)
- Coverage: ASCII only, 89 mapped code points

## HigherPixels — `HigherPixels-Regular.ttf`

- Author: Max Infeld, Xerographer Fonts, xerographer.blogspot.com
- Licence: all rights reserved, contact maxinfeld@gmail.com
- Embedding permissions declared by the file: editable embedding (`fsType` 8)
- Coverage: letters and digits, 70 mapped code points, no accents at all

This family is used only by the wordmark, which reads `GIN RUMMY`, so the letters
it lacks never reach the screen through it.

## Consequence for the project

Neither Pixelta nor HigherPixels carries accented letters, an inverted question or
exclamation mark, or `U+25CF`, which is the character the password field draws. The
families declared in `Styles/Theme.xaml` therefore name Pixeloid Sans as the next
family in the list, and WPF resolves each missing glyph there. Without that list
the accents of STK-13 would fall to a system font and would not look like part of
the same typeface.

Neither Pixelta nor HigherPixels grants redistribution in writing. The project is
academic and produces no income, which is what their terms describe, but if the
course requires every asset to carry explicit redistribution rights both should be
replaced by openly licensed pixel families. Pixeloid Sans already in this folder
can take over the display role, and Silkscreen, Press Start 2P and Pixelify Sans
are alternatives under the same Open Font License.

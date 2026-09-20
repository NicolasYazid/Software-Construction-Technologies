# GinRummy.Client — GUI internacionalizada

Cliente WPF del videojuego Gin Rummy 2D. Cubre el ciclo de cuenta y acceso, internacionalizado
en `es-MX` y `en-US`.

Equipo 6 · Tecnologías para la Construcción de Software · Universidad Veracruzana.

## Cómo abrirlo

```bash
git clone https://github.com/NicolasYazid/Software-Construction-Technologies.git
```

Abre `GinRummy.sln` con Visual Studio 2019 o 2022. Necesitas la carga de trabajo **Desarrollo de
escritorio de .NET** con el **paquete de destino de .NET Framework 4.8**. No hay paquetes NuGet ni
dependencias externas: el repositorio se compila tal como se clona.

## Alcance

Nueve de las veintiséis pantallas del sistema, que es el 34.6 % por pantallas y el 26.5 % por
cadenas. Son las del ciclo de cuenta y acceso, así que cubren las operaciones de alta, lectura y
modificación del jugador.

| Pantalla | Ventana | Caso de uso |
| :--- | :--- | :--- |
| P00 | Bloque compartido | Transversal |
| P01 | `GuiMainMenu` | Entrada al cliente y selector de idioma |
| P02 | `GuiSignUp` | CU-01 |
| P03 | `GuiVerifyEmail` | CU-09, y la verificación de CU-01, CU-07 y CU-08 |
| P04 | `GuiAccountCreated` | Cierre de CU-01 |
| P05 | `GuiLogIn` | CU-02 |
| P06 | `GuiTwoStep` | CU-02 FA-04, CU-04, CU-06 |
| P07 | `GuiRecoverPassword` | CU-08 |
| P08 | `GuiNewPassword` | CU-06 y cierre de CU-08 |

`GuiNewPassword` sirve a dos flujos con una sola ventana: en CU-06 pide la contraseña actual antes
de la nueva, y en CU-08 la oculta, porque el jugador llega ahí precisamente cuando ya no la
recuerda. El encabezado y el título de la ventana salen de una clave distinta en cada flujo.

## Diseño

El prototipo de Figma fue el punto de partida y sigue siendo la referencia de qué pantallas
existen y qué contiene cada una. **A partir de esta versión el aspecto visual es una decisión del
equipo y vive en el código, no en el archivo de Figma.** Los identificadores de pantalla (P01 a
P25) se conservan porque los casos de uso los citan paso por paso; lo que ya no se conserva es la
obligación de calcar colores, tipografías y coordenadas.

Los tokens de color, las familias tipográficas y los estilos de control están centralizados en
`Styles/Theme.xaml`. Cambiar el aspecto de una pantalla es cambiar un token, nunca tocar el XAML
de la ventana ni el código.

Las tipografías (Oswald y Source Sans 3, ambas bajo SIL Open Font License) van embebidas en
`Fonts/` y se cargan por URI de paquete, así que la aplicación se ve igual en cualquier máquina
sin instalar nada.

### Pendiente de unificación

Las pantallas arrastran dos generaciones de estilos: P01, P02, P03 y P05 usan una paleta y un
radio de campo, y P04, P06, P07 y P08 usan otros. Está aislado en `Theme.xaml` y se resuelve ahí,
en los estilos con prefijo `StyPlain`.

## El fondo líquido del menú

`Controls/CtlLiquidBackground` dibuja el fondo animado de `GuiMainMenu`. Está escrito desde cero,
como exige CON-06, y no incorpora código de terceros ni reproduce el fondo de ningún producto
existente, que es lo que pide el asesor legal (STK-12).

**Cómo funciona.** Cada fotograma se calcula en el procesador sobre un mapa de bits pequeño que el
control estira a su propio tamaño. Por cada píxel se aplica un remolino en coordenadas polares
cuya fuerza crece hacia el centro, después tres plegados de deformación del dominio con seno y
coseno, y por último se mezcla la paleta: la banda resultante interpola entre el verde profundo y
el verde medio, y las crestas suman el verde claro. Un viñeteado oscurece los bordes.

**Por qué en el procesador y no con un shader.** WPF sí admite shaders de píxel, pero el perfil
`ps_2_0` no da para un efecto de este tamaño y `ps_3_0` solo se dibuja cuando hay render por
hardware: en una máquina sin GPU, en una máquina virtual o por escritorio remoto, WPF cae a render
por software y el efecto desaparece sin avisar. Como el sistema tiene que ejecutarse para entrar a
revisión (CON-12), se prefirió un camino que se ve igual en cualquier equipo. El costo es un hilo
de procesador mientras el menú está visible, y el control se detiene solo cuando la ventana deja
de verse o se cierra.

**Cómo cambiar la paleta.** Los tres colores son propiedades del control y se fijan desde el XAML
del menú con los tokens `ClrMenuLiquidDeep`, `ClrMenuLiquidMid` y `ClrMenuLiquidGlow` de
`Theme.xaml`. La paleta pertenece a la pantalla, no al control, así que el mismo fondo puede
reutilizarse en el lobby con otros colores sin tocar una línea de código.

## Cómo funciona la internacionalización

**Los recursos.** `Resources/Strings.resx` tiene las 351 cadenas en `es-MX` y es el archivo
neutro, declarado con `[assembly: NeutralResourcesLanguage("es-MX")]`, así que viaja dentro del
ensamblado principal. `Resources/Strings.en-US.resx` compila como ensamblado satélite. Agregar un
idioma es agregar un `.resx` y una fila en `LocalizationProvider.AvailableCultures`: cero líneas
de C# modificadas y cero recompilaciones de la lógica, que es lo que exige CON-16.

**El proveedor.** `LocalizationProvider` resuelve cada clave contra el `ResourceManager` de la
cultura activa, expone un indexador e implementa `INotifyPropertyChanged`. Al cambiar de idioma
libera la caché y notifica `Item[]`: todos los enlaces de todas las ventanas abiertas se refrescan
sin reabrir nada.

**Los enlaces.** Ningún texto visible está escrito en el XAML. Cada uno usa la forma que fija el
estándar de codificación del equipo:

```xml
<Button x:Name="btnLogIn"
        Content="{Binding [Shared_BtnLogIn], Source={StaticResource Loc}}" />
```

Los textos que se dibujan en mayúsculas pasan por `UpperCaseConverter`, que convierte con la
cultura activa y no con la invariante, porque la forma mayúscula de una letra depende del idioma.
El recurso se guarda siempre en su forma natural.

**Los valores con marcador.** Nada se concatena. `LocalizationProvider.Format` aplica
`string.Format` con la cultura activa, y las ventanas con contadores, duraciones o títulos
dependientes del flujo sobrescriben `RefreshFormattedText`, que `GuiWindowBase` invoca en cada
cambio de cultura.

**El logotipo.** `CtlBrandMark` lee la clave de marca del diccionario y la parte en su espacio para
colocar el símbolo en medio. No hay texto de marca escrito en el XAML.

## Cómo probar el cambio de idioma

El selector vive en `GuiMainMenu`. El menú es la ventana principal y las demás se abren sin
bloquearlo, así que puedes cambiar de idioma con otra pantalla abierta y ver el refresco en
caliente.

1. **Los textos cambian.** Abre una pantalla, vuelve al menú, cambia el idioma y comprueba que la
   pantalla abierta se traduce sin reabrirse.
2. **No hay textos sin traducir.** Recorre las nueve pantallas en las dos culturas.
3. **Nada se corta.** El inglés es más corto que el español en casi todas las cadenas, así que el
   caso crítico es el inverso. Vigila `Shared_ChkKeepSignedIn`, `LogIn_LnkForgotPassword`,
   `NewPassword_BtnUpdate` y `VerifyEmail_BtnVerifyLater`.
4. **La distribución se conserva.** Todo texto lleva `TextWrapping`, así que la expansión crece en
   alto y no recorta.
5. **Los formatos culturales.** El pie del menú muestra la hora con la clave `Shared_Timestamp`;
   los temporizadores de P03 y P06 corren en vivo. Cambia la cultura y compara.
6. **Los títulos de ventana.** Están enlazados al recurso igual que el contenido, así que también
   cambian de idioma. En P08 además cambian según el flujo.

## Lo que queda fuera

- **La lógica.** Ninguna ventana valida reglas de negocio, abre conexiones ni consulta la base de
  datos. La única comprobación que corre en el cliente es la coincidencia de la contraseña con su
  confirmación, que es la única que CU-01 FA-04 permite resolver sin enviar nada al servidor.
- **Las claves de pantallas posteriores.** Los dos `.resx` ya tienen las 351 cadenas del
  diccionario, pero las que pertenecen al lobby y a la partida no están cableadas todavía porque
  sus pantallas no existen en esta entrega.
- **El diseñador de XAML** mostrará los textos en blanco, porque el recurso `Loc` se publica al
  arrancar la aplicación. En ejecución se ven correctamente.

## Estructura

```
GinRummy.sln
src/GinRummy.Client/
  App.xaml, App.xaml.cs             Publica el proveedor de localizacion
  Properties/AssemblyInfo.cs        Declara es-MX como cultura neutra
  Fonts/                            Oswald y Source Sans 3 embebidas
  Resources/Strings.resx            351 cadenas en es-MX (cultura base)
  Resources/Strings.en-US.resx      351 cadenas en en-US (ensamblado satelite)
  Localization/                     Proveedor de localizacion y opciones de cultura
  Converters/                       Texto guia y mayusculas por cultura
  Controls/CtlBrandMark             Logotipo de la marca
  Controls/CtlLiquidBackground      Fondo liquido animado del menu
  Styles/Theme.xaml                 Tokens de color, tipografia y estilos de control
  Styles/Icons.xaml                 Vectores de los iconos
  Views/                            GuiWindowBase y las ocho pantallas
```

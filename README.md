# GinRummy.Client — GUI internacionalizada (Actividad 2)

Cliente WPF del videojuego Gin Rummy 2D. Esta entrega cubre el ciclo de cuenta y acceso
del prototipo, con la interfaz internacionalizada en `es-MX` y `en-US`.

Equipo 6 · Tecnologías para la Construcción de Software.

## Cómo abrirlo

1. Abrir `GinRummy.sln` con Visual Studio 2019 o 2022.
2. Comprobar que está instalada la carga de trabajo **Desarrollo de escritorio de .NET**
   con el **paquete de destino de .NET Framework 4.8**.
3. Compilar y ejecutar. El proyecto no usa paquetes NuGet ni dependencias externas.

El proyecto es WPF sobre **.NET Framework 4.8**, en formato de proyecto clásico, no SDK.
No es .NET 8 ni .NET Core.

## Alcance congelado

Nueve de las veintiséis pantallas del prototipo, que son el ciclo completo de cuenta y
acceso: **34.6 % por pantallas** y **26.5 % por cadenas** (93 de 351).

| Pantalla | Ventana | Caso de uso |
| :--- | :--- | :--- |
| P00 | Bloque compartido (sin ventana propia) | Transversal |
| P01 | `GuiMainMenu` | Entrada al cliente |
| P02 | `GuiSignUp` | CU-01 |
| P03 | `GuiVerifyEmail` | CU-09, y la verificación de CU-01, CU-07 y CU-08 |
| P04 | `GuiAccountCreated` | Cierre de CU-01 |
| P05 | `GuiLogIn` | CU-02 |
| P06 | `GuiTwoStep` | CU-02 FA-04, CU-04, CU-06 |
| P07 | `GuiRecoverPassword` | CU-08 |
| P08 | `GuiNewPassword` | CU-06 y cierre de CU-08 |

## Cómo funciona la internacionalización

**Los recursos.** `Resources/Strings.resx` contiene las 351 cadenas del diccionario en
`es-MX` y es el archivo neutro: `AssemblyInfo.cs` lo declara con
`[assembly: NeutralResourcesLanguage("es-MX")]`, de modo que viaja dentro del ensamblado
principal. `Resources/Strings.en-US.resx` se compila como ensamblado satélite en
`bin\Debug\en-US\`. Agregar un idioma nuevo es agregar un `.resx` más y una fila en
`LocalizationProvider.AvailableCultures`: no hay una sola cadena visible en el código de
las ventanas.

**El proveedor.** `Localization/LocalizationProvider.cs` es un singleton que resuelve cada
clave contra el `ResourceManager` de la cultura activa. Expone un indexador, que es lo que
usan los enlaces del XAML, e implementa `INotifyPropertyChanged`. Al cambiar de cultura
libera los recursos en caché y notifica `Item[]`, con lo que **todos los enlaces de todas
las ventanas abiertas se refrescan sin reabrir nada**.

**Los enlaces.** `App.xaml.cs` publica el proveedor como recurso de aplicación bajo la
clave `Loc`, y cada texto visible se enlaza en la forma que fija el estándar de
codificación del equipo:

```xml
<Button x:Name="btnLogIn"
        Content="{Binding [Shared_BtnLogIn], Source={StaticResource Loc}}" />
```

**Los títulos de ventana** también se enlazan, con la misma forma.

**Los valores con marcador.** Ninguna frase se arma por concatenación.
`LocalizationProvider.Format` aplica `string.Format` con la cultura activa, y las ventanas
que muestran contadores o duraciones sobrescriben `RefreshFormattedText`, que `GuiWindowBase`
invoca en cada cambio de cultura. Las duraciones se formatean con
`TimeSpan.ToString(formato, CurrentCulture)` y la hora con `DateTime.ToString("t", CurrentCulture)`.

**El texto guía de los campos** es una cadena localizada aparte, dibujada en un `TextBlock`
detrás del campo y gobernada por `EmptyTextToVisibilityConverter`. Nunca sustituye a la
etiqueta del campo.

## Qué NO entra a los recursos

Se respetó la hoja *No traducibles* del diccionario. En concreto, en estas nueve pantallas:

- La máscara de contraseña la dibuja el `PasswordBox`; no es una cadena.
- El glifo del botón que muestra u oculta la contraseña se dibuja como contenido gráfico,
  fuera de toda cadena traducible. El significado viaja en su `ToolTip`, que sí está
  localizado (`Shared_TipTogglePassword`).
- Los nombres de las culturas en el selector de idioma van escritos en su propio idioma
  (`Español (México)`, `English (United States)`) y por eso no se traducen.
- Las horas, los temporizadores y el contador de intentos son datos numéricos o temporales:
  se formatean con `CultureInfo` y se mantienen fuera de la cadena.
- La marca existe como `Shared_AppTitle` por si algún mercado la exigiera, pero vale igual
  en las dos culturas.

## Cómo probar el cambio de idioma

El selector de idioma vive en `GuiMainMenu` (P01), tal como lo define el prototipo. El menú
principal es la ventana principal y las demás se abren sin bloquearlo, de modo que se puede
cambiar de cultura con otra pantalla abierta y comprobar el refresco en caliente.

Lista de comprobación, alineada con los puntos de validación de la actividad:

1. **Los textos cambian según la cultura.** Abrir cualquier pantalla, volver al menú,
   cambiar el idioma y comprobar que la pantalla abierta se traduce sin reabrirse.
2. **No hay textos sin traducir.** Recorrer las nueve pantallas en las dos culturas.
3. **Nada se corta.** El inglés es más corto que el español en casi todas las cadenas, así
   que el caso crítico es el contrario: comprobar `Shared_ChkKeepSignedIn`,
   `SignUp_LblSubtitle`, `NewPassword_BtnUpdate` y `VerifyEmail_BtnVerifyLater`.
4. **La distribución se conserva.** Las tarjetas tienen ancho fijo y todo texto lleva
   `TextWrapping`, así que la expansión crece en alto y no rompe la columna.
5. **Los formatos culturales.** El pie del menú principal muestra la hora con la clave
   `Shared_Timestamp`; los temporizadores de P03 y P06 corren en vivo. Cambiar la cultura
   y comparar el formato de la hora.

## Lo que queda fuera de esta entrega

- **23 de las 93 claves del alcance no están cableadas todavía**, y es correcto que así sea:
  doce son claves compartidas de P00 que pertenecen a pantallas del lobby (P11 en adelante),
  y once son mensajes de error que emite el servidor y que se mostrarán en `lblErrorMessage`
  cuando existan los controladores. Las 351 claves ya están en los dos `.resx`.
- **La lógica.** Ninguna ventana valida reglas, abre conexiones ni consulta la base de datos.
  La única comprobación que corre en el cliente es la coincidencia de contraseñas, porque es
  la única que el CU-01 FA-04 permite resolver sin enviar nada al servidor.
- **El diseñador de XAML de Visual Studio** mostrará los textos en blanco, porque el recurso
  `Loc` se publica al arrancar la aplicación. En ejecución se ven correctamente.
- **Los colores y la tipografía** están centralizados en `Styles/Theme.xaml` como tokens.
  Hay que ajustarlos contra el prototipo de Figma; el contenido y la estructura de cada
  pantalla sí salen del catálogo de cadenas.

## Estructura

```
GinRummy.sln
src/GinRummy.Client/
  App.xaml, App.xaml.cs            Publica el proveedor de localización
  Properties/AssemblyInfo.cs       Declara es-MX como cultura neutra
  Resources/Strings.resx           351 cadenas en es-MX (cultura base)
  Resources/Strings.en-US.resx     351 cadenas en en-US (ensamblado satélite)
  Localization/                    Proveedor de localización y opciones de cultura
  Converters/                      Visibilidad del texto guía
  Styles/Theme.xaml                Tokens de diseño y estilos
  Views/                           GuiWindowBase y las ocho pantallas
```

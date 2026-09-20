# GinRummy.Client — GUI internacionalizada (Actividad 2)

Cliente WPF del videojuego Gin Rummy 2D. Cubre el ciclo de cuenta y acceso del prototipo,
internacionalizado en `es-MX` y `en-US`, y reconstruido contra el archivo de Figma.

Equipo 6 · Tecnologías para la Construcción de Software.

## Dónde ponerlo

Descomprime la carpeta `GinRummyClient` dentro de tu repositorio:

```
C:\Users\neptu\Desktop\ESCUELA DESCARGAS\git\Software-Construction-Technologies\
```

Luego abre `GinRummyClient\GinRummy.sln` con Visual Studio 2019 o 2022. Necesitas la carga de
trabajo **Desarrollo de escritorio de .NET** con el **paquete de destino de .NET Framework 4.8**.
No hay paquetes NuGet ni dependencias externas.

## Alcance congelado

Nueve de las veintiséis pantallas: **34.6 % por pantallas** y **26.5 % por cadenas** (93 de 351).

| Prototipo | Nodo de Figma | Ventana | Caso de uso |
| :--- | :--- | :--- | :--- |
| P00 | — | Bloque compartido | Transversal |
| P01 | `46:1646` | `GuiMainMenu` | Entrada al cliente |
| P02 | `61:1993` | `GuiSignUp` | CU-01 |
| P03 | `80:1320` | `GuiVerifyEmail` | CU-09, y la verificación de CU-01, CU-07 y CU-08 |
| P04 | `83:2301` | `GuiAccountCreated` | Cierre de CU-01 |
| P05 | `61:2080` | `GuiLogIn` | CU-02 |
| P06 | `83:2309` | `GuiTwoStep` | CU-02 FA-04, CU-04, CU-06 |
| P07 | `80:1460` | `GuiRecoverPassword` | CU-08 |
| P08 | `80:1478` | `GuiNewPassword` | CU-06 y cierre de CU-08 |

## Qué se tomó del prototipo

**Los colores, uno por uno.** No hay un solo color inventado. Los tokens de `Styles/Theme.xaml`
son los valores del archivo: `#E9E9E9` del lienzo del menú, `#F6F6F6` del fondo de las pantallas
con tarjeta, `#727272` de los títulos y del botón primario, `#9B9B9B` de las etiquetas de campo,
`#E3E3E3` del borde de los campos, `#8E8E93` del botón grande del menú, `#D9D9D9` del panel
lateral de ilustración.

**La geometría.** Cada ventana tiene el tamaño exacto de su marco en Figma, y los márgenes,
altos y anchos salen de las coordenadas del archivo: campos de 50 px con radio 3 y padding 16 en
las pantallas con tarjeta, de 48 px con radio 4 y padding 15 en las demás; tarjeta de 471 px con
separación de 20 entre bloques; botones de 50, 52 y 64 px según la pantalla.

**Los vectores.** Los seis iconos (correo, usuario, candado, ojo, globo y la diana del logotipo)
se exportaron del archivo con la API de plugins de Figma y se convirtieron a geometría de WPF en
`Styles/Icons.xaml`. Los datos de trazo son los del prototipo, no se redibujaron a mano.

**Las tipografías.** Van embebidas en `Fonts/` y se cargan por URI de paquete, así que la
aplicación se ve igual en cualquier máquina sin instalar nada. Oswald es la del prototipo. Ambas
familias son de licencia SIL Open Font License.

**Los nombres de los controles.** El prototipo ya nombra sus capas con las claves del diccionario
y con los identificadores de control. Los `x:Name` del XAML son esos mismos: `txtEmail`,
`pwdPassword`, `chkKeepSignedIn`, `lnkForgotPassword`, `btnUpdatePassword`, `cmbLanguage`.
Coinciden con la columna Control del diccionario y con el estándar de codificación.

## Dos generaciones de diseño en el prototipo

Vale la pena que lo sepan porque no es un error de la implementación. P01, P02, P03 y P05 usan
Source Sans Pro con una paleta afinada (títulos `#727272`, campos con borde `#E3E3E3`, radio 3).
P04, P06, P07 y P08 son de una generación anterior: usan Inter, títulos `#666666`, bordes
`#D9D9D9`, radio 4 y botón `#757575`. Cada pantalla quedó como está en el archivo, con dos
familias de estilos en el tema, para no alterar el diseño. Si deciden unificarlo, el cambio es
solo en `Theme.xaml`.

## Desviaciones conscientes

Son cinco, todas documentadas aquí para que nadie las descubra en la revisión.

1. **Interletraje.** El prototipo aplica tracking de 0.45 a 1.28 px en los textos en mayúsculas.
   `TextBlock` de WPF no tiene propiedad de interletraje, así que no se aplicó. La diferencia es
   de menos de un píxel y medio por carácter.
2. **Inter.** No existe una versión estática distribuible de Inter para embeber, así que las
   cuatro pantallas de la generación anterior usan Source Sans 3. La geometría y los colores de
   esas pantallas sí son los del prototipo.
3. **Source Sans Pro.** Adobe ya no distribuye los archivos estáticos de Source Sans Pro; se
   embebió Source Sans 3, que es su sucesor directo y comparte métricas.
4. **El patrón de cruz del menú.** Las dos capas `_cross` del fondo de P01 se omitieron: son del
   mismo tono que el lienzo y no se ven en el render del propio prototipo. El borde punteado sí
   está.
5. **Las esquinas redondeadas.** Los marcos de Figma tienen radio 30, que es el encuadre del
   mockup. Las ventanas usan el marco normal de Windows.

Además, dos elementos del prototipo que se respetaron tal cual: el botón vacío de opciones del
menú principal existe como caja sin texto porque todavía no tiene caso de uso, y el botón de jugar
como invitado de P05 está oculto en el archivo, así que no se dibujó.

## Cómo funciona la internacionalización

**Los recursos.** `Resources/Strings.resx` tiene las 351 cadenas en `es-MX` y es el archivo
neutro, declarado con `[assembly: NeutralResourcesLanguage("es-MX")]`, así que viaja dentro del
ensamblado principal. `Resources/Strings.en-US.resx` compila como ensamblado satélite. Agregar un
idioma es agregar un `.resx` y una fila en `LocalizationProvider.AvailableCultures`.

**El proveedor.** `LocalizationProvider` resuelve cada clave contra el `ResourceManager` de la
cultura activa, expone un indexador e implementa `INotifyPropertyChanged`. Al cambiar de idioma
libera la caché y notifica `Item[]`: todos los enlaces de todas las ventanas abiertas se refrescan
sin reabrir nada.

**Los enlaces.** Cada texto visible usa la forma que fija el estándar del equipo:

```xml
<Button x:Name="btnLogIn"
        Content="{Binding [Shared_BtnLogIn], Source={StaticResource Loc}}" />
```

Los textos que el prototipo dibuja en mayúsculas pasan por `UpperCaseConverter`, que convierte con
la cultura activa y no con la invariante, porque la forma mayúscula de una letra depende del
idioma. El recurso se guarda en su forma natural.

**Los valores con marcador.** Nada se concatena. `LocalizationProvider.Format` aplica
`string.Format` con la cultura activa, y las ventanas con contadores o duraciones sobrescriben
`RefreshFormattedText`, que `GuiWindowBase` invoca en cada cambio de cultura.

**El logotipo.** `CtlBrandMark` lee la clave de marca del diccionario y la parte en su espacio
para colocar la diana en medio, que es lo que el prototipo hace con espaciado. No hay texto de
marca escrito en el XAML.

## Cómo probar el cambio de idioma

El selector vive en `GuiMainMenu`, como en el prototipo. El menú es la ventana principal y las
demás se abren sin bloquearlo, así que puedes cambiar de idioma con otra pantalla abierta y ver el
refresco en caliente.

1. **Los textos cambian.** Abre una pantalla, vuelve al menú, cambia el idioma y comprueba que la
   pantalla abierta se traduce sin reabrirse.
2. **No hay textos sin traducir.** Recorre las nueve pantallas en las dos culturas.
3. **Nada se corta.** El inglés es más corto que el español en casi todas las cadenas, así que el
   caso crítico es el inverso. Vigila `Shared_ChkKeepSignedIn`, `LogIn_LnkForgotPassword`,
   `NewPassword_BtnUpdate` y `VerifyEmail_BtnVerifyLater`.
4. **La distribución se conserva.** Los anchos son los del prototipo y todo texto lleva
   `TextWrapping`, así que la expansión crece en alto.
5. **Los formatos culturales.** El pie del menú muestra la hora con la clave `Shared_Timestamp`;
   los temporizadores de P03 y P06 corren en vivo. Cambia la cultura y compara.

## Lo que queda fuera

- **23 de las 93 claves del alcance no están cableadas**, y es correcto: doce son claves
  compartidas de P00 que pertenecen al lobby (P11 en adelante) y once son mensajes que emite el
  servidor. Las 351 ya están en los dos `.resx`.
- **La lógica.** Ninguna ventana valida reglas, abre conexiones ni consulta la base de datos. La
  única comprobación que corre en el cliente es la coincidencia de contraseñas, que es la única
  que CU-01 FA-04 permite resolver sin enviar nada al servidor.
- **El diseñador de XAML** mostrará los textos en blanco, porque el recurso `Loc` se publica al
  arrancar la aplicación. En ejecución se ven correctamente.

## Estructura

```
GinRummy.sln
src/GinRummy.Client/
  App.xaml, App.xaml.cs          Publica el proveedor de localización
  Properties/AssemblyInfo.cs     Declara es-MX como cultura neutra
  Fonts/                         Oswald y Source Sans 3 embebidas
  Resources/Strings.resx         351 cadenas en es-MX (cultura base)
  Resources/Strings.en-US.resx   351 cadenas en en-US (ensamblado satélite)
  Localization/                  Proveedor de localización y opciones de cultura
  Converters/                    Texto guía y mayúsculas por cultura
  Controls/CtlBrandMark          Logotipo con la diana del prototipo
  Styles/Theme.xaml              Tokens y estilos de las dos generaciones
  Styles/Icons.xaml              Vectores exportados de Figma
  Views/                         GuiWindowBase y las ocho pantallas
```

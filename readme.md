# Memoria A*

## **Estudio de la Teoría**

Como primer paso se estudia la teoría del algoritmo, principalmente usando de referencia su página de Wikipedia (en inglés). La cual además proporciona pseudocódigo del algoritmo.

## **Implementación en C#**

Tras tener claro como implementarlo, y con el pseudocódigo al lado, se crea una clase llamada `Pathfinder` en C# que replica el algoritmo, en principio usando las celdas de un `Tileset` de Unity. Tras terminar el algoritmo, se decide generalizarlo para 3 dimensiones, para ser capaz de contemplar rutas mas complicadas y en entornos 3D. 

## **Creación de un *Grid* en 3D**

En lugar de los `Tileset` 2D propios de Unity, para distribuir los nodos del algoritmo en un espacio tridimensional, se crea la clase `Grid3D` en C# para traducir el espacio continuo 3D de Unity, a casillas cúbicas discretas con lado 1 unidad de mediad de Unity.

Esta clase es capaz de convertir entre coordenadas en `WorldSpace` de Unity y casillas del `Grid3D`.

## Preparación de la escena y *assets* en Unity

Se crea un `Prefab` basado en un cubo 1x1x1 capaz de ocupar exactamente una casilla de nuestro espacio discreto en el que se va a navegar. Lleva un script que llamamos `NavBlock` con las coordenadas de la casilla que ocupa y sus vecinos en el grafo.

Se construye una escena en Unity con un objeto vacío que funcionará como controlador del juego y de la cuadricula 3D, lleva la clase `Grid3D` y funcionalidades por `raycast` para crear y borrar cubos `NavBlock` en la escena durante la ejecución del juego con los botones izquierdo y derecho del ratón.

Nuestro entorno 3D esta definido por estos cubos, pero los cubos en si no son las posiciones navegables por nuestro agente con A*, si no las casillas vacías  adyacentes a los cubos. La idea es que el agente pueda navegar por encima de los cubos(andar) y por sus lados (equivalente a trepar), pero no por debajo(no puede trepar por el techo). Y que trepar tenga un coste de navegación mayor que andar.

## Generación del grafo de nodos navegables

Con esta premisa sobre como queremos que entienda el algoritmo A* nuestro entorno, creamos un método `BakeMesh()` que toma todos los cubos `NavBlock` presentes en la escena, y calcula que nodos son navegables y cual es su coste, guardándolos en un diccionario de nodos y costes adaptado al Input de nuestra implementación del algoritmo A*.

## Integración del algoritmo a la escena

Se añade también a la escena una esfera con el script de `Pathfinder`. 

Se añade un elemento de interfaz para poder cambiar entre dos modos: uno para editar los cubos del escenario, y otro para seleccionar un camino a recorrer por la esfera. Cuando entramos en modo “pathfinding” ejecutamos el método `BakeMesh()` .

Con el escenario creado y el algoritmo ya capaz de darnos un `Stack` con todos los nodos de un camino a recorrer, se escribe una función en la clase `Grid3D` para que al hacer clic en alguna cara de un `NavBlock`  se muestre una previsualización del camino entre la esfera `Pathfinder` y el nodo adyacente a la cara seleccionada.

## Ajustes y mejoras

En este punto, ya se puede testear el software y previsualizar los caminos de la esfera tomaría en el escenario dibujable por nosotros.

Tras algo de experimentación, se añaden un par de funcionalidades para seguir experimentando con el algoritmo: Se permite seleccionar la heurística a usar por A* en el editor de Unity mediante una variable `Enum` y se empiezan a aplicar algunas correcciones para que los caminos resultantes por cada heurística sean mas naturales:

- Se añade un coste adicional al pasar a un nodo a una altura superior al actual. Esto representa que el coste de trepar es mayor ascendiendo.
- Se añade un coste a los movimientos diagonales para que Chebyshev no zigzaguee.
- Se Limita Manhattan para solo moverse en direcciones ortogonales y preferir caminos zigzagueantes.

Tras aplicar las correcciones y tras más experimentación, se balancean los costes de los nodos, y se dejan de considerar nodos válidos las casillas únicamente adyacentes en las esquinas de los cubos, que hacían ver que la esfera estaba completamente en el aire.

### UI y otros ajustes visuales

Se añade la funcionalidad para que la esfera recorra el camino seleccionado al hacer clic derecho, y se ajusta la velocidad de la esfera en función del coste del nodo que esta recorriendo, para que expresar el esfuerzo extra de trepar.

Hasta este punto la cámara del juego era fija, y eso limita colocar bloques y molesta para visualizar caminos complicados, por lo que se integra un script básico a la cámara para moverla libremente. 

Además se añaden un par de paneles de UI mostrando las diferentes acciones que se pueden tomar y los mapas de botones para cambiar de modo. Además se añaden mas opciones para desactivar las correcciones de las heurísticas, desactivar la habilidad de trepar, o permitir que las esquinas sean trepables.

## Diagnósticos de rendimiento

Como último retoque al proyecto, se añade en la UI información sobre el coste computacional del algoritmo, para poder comparar lo eficiente que es cada heurística o como responde el algoritmo a diferentes configuraciones de nodos.

# Comparativas

 A continuación una serie de tablas comparando el rendimiento de las diferentes heurísticas y configuraciones del algoritmo.

- **Nodos visitados** son los nodos que el algoritmo ha tenido que buscar antes de encontrar una solución.
- El **tiempo de procesado** son los ciclos de reloj interno del procesador que ha llevado encontrar una solución para el algoritmo.

## Línea Recta

![image.png](image.png)

| Heurística | Nodos visitados | Tiempo de procesado(Ticks de CPU) |
| --- | --- | --- |
| Manhattan | 11 | 564 |
| Manhattan (Con correcciones) | 15 | 706 |
| Chebyshev | 29 | 1712 |
| Chebyshev (Con correcciones) | 11 | 727 |
| Euclídea | 19 | 1129 |
| Euclidea (Con correcciones) | 11 | 750 |
| Dijkstra (Ninguna heurística) | 157 | 7595 |

## Trepar pared

![image.png](image%201.png)

| Heurística | Nodos visitados | Tiempo de procesado(Ticks de CPU) |
| --- | --- | --- |
| Manhattan | 570 | 30169 |
| Manhattan (Con correcciones) | 442 | 21488 |
| Chebyshev | 893 | 104406 |
| Chebyshev (Con correcciones) | 847 | 101290 |
| Euclídea | 891 | 102706 |
| Euclidea (Con correcciones) | 836 | 99325 |
| Dijkstra (Ninguna heurística) | 954 | 118101 |

## Ir al piso de abajo con escaleras.

![image.png](image%202.png)

| Heurística | Nodos visitados | Tiempo de procesado(Ticks de CPU) |
| --- | --- | --- |
| Manhattan | 164 | 5813 |
| Manhattan (Con correcciones) | 158 | 6036 |
| Chebyshev | 57 | 4520 |
| Chebyshev (Con correcciones) | 61 | 5711 |
| Euclídea | 31 | 2502 |
| Euclidea (Con correcciones) | 43 | 3526 |
| Dijkstra (Ninguna heurística) | 156 | 15912 |

## Camino complejo con múltiples opciones y desniveles.

![image.png](image%203.png)

### Trepar permitido:

| Heurística | Nodos visitados | Tiempo de procesado(Ticks de CPU) |
| --- | --- | --- |
| Manhattan | 768 | 42833 |
| Manhattan (Con correcciones) | 795 | 37506 |
| Chebyshev | 442 | 104633 |
| Chebyshev (Con correcciones) | 712 | 138809 |
| Euclídea | 341 | 73094 |
| Euclidea (Con correcciones) | 617 | 111803 |
| Dijkstra (Ninguna heurística) | 1001 | 146478 |

### Trepar prohibido:

| Heurística | Nodos visitados | Tiempo de procesado(Ticks de CPU) |
| --- | --- | --- |
| Manhattan | Camino Imposible | Camino Imposible |
| Manhattan (Con correcciones) | Camino Imposible | Camino Imposible |
| Chebyshev | 264 | 15406 |
| Chebyshev (Con correcciones) | 267 | 10878 |
| Euclídea | 260 | 14543 |
| Euclidea (Con correcciones) | 266 | 10536 |
| Dijkstra (Ninguna heurística) | 301 | 11075 |

## Observaciones sobre los datos

Analizando los datos, se observan ciertos fenómenos, algunos esperados, y otros más sorprendentes y difíciles de explicar.

- La heurística manhattan, que solo permite movimiento ortogonal, sueles ser mas barata en caminos simples, pues tienes menos nodos entre los que buscar. Sin embargo en caminos mas complicados y con tridimensionalidad, se acerca mas al rendimiento de otros algoritmo o incluso es menos eficiente.
- Las correcciones hacen que los algoritmos prefieran caminos mas simples y bonitos, por tanto cuando la solución optima de un camino puede conseguirse con un camino con pocos desvíos, las correcciones hacen mas eficiente a la heurística, pero cuando una solución requiere de un camino “feo” las correcciones hacen que la heurística sea peor, pues da preferencia a analizar nodos que no van ser ideales para el camino de la solución.
- En el ultimo experimento, con un camino muy largo y complicado y sin trepar, Dijkstra termina el cálculo mas rápido que las heurísticas Chebyshev y Euclídea, a pesar de recorrer mas nodos. Este es el resultado mas chocante pues supuestamente las heurísticas son admisibles y deberían siempre ser mejores que ninguna heurística. Es importante destacar que esto no rompe la admisibilidad, pues la teoría solo asegura que encuentre la solución en menos nodos que sin heurística, cosa que hace. El hecho de que tarde mas tiempo debe ser debido a una combinación de factores:
    - El camino es tal que los algoritmos con heurísticas tienen que buscar en prácticamente todos los nodos de la red, acercándose mucho al rendimiento de Dijkstra.
    - El cálculo de la heurística toma tiempo de CPU, al contrario que sin heurística, que tiene un calculo menos que realizar por nodo.
    
    Por tanto aunque las heurísticas hayan encontrado el camino en menos nodos, en casos extremos como este, es posible que tarden mas que ninguna heurística por los cálculos adicionales que hacen cada nodo para medir la distancia.
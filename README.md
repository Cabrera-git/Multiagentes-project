# Simulador de tráfico urbano 
## TC2008B
Por Diego Corrales A01781631 | Do Hyun Nam A01025276 | Andrew Dunkerley A01025076 | Emiliano Cabrera A01025453
## Reto
La movilidad urbana, se define como la habilidad de transportarse de un lugar a otro y es fundamental para el desarrollo económico y social y la calidad de vida de los habitantes de una ciudad. Desde hace un tiempo, asociar la movilidad con el uso del automóvil ha sido un signo distintivo de progreso. Sin embargo, esta asociación ya no es posible hoy. El crecimiento y uso indiscriminado del automóvil —que fomenta políticas públicas erróneamente asociadas con la movilidad sostenible— genera efectos negativos enormes en los niveles económico, ambiental y social en México.

El reto consiste en proponer una solución al problema de movilidad urbana en México, mediante un enfoque que reduzca la congestión vehicular al simular de manera gráfica el tráfico, representando la salida de un sistema multi agentes.

Imagina una solución que implemente una de las siguientes estrategias de ejemplo:

- Controlar y asignar los espacios de estacionamiento disponible en una zona de la ciudad, evitando así que los autos estén dando vueltas para encontrar estacionamiento.
- Compartir tu vehículo con otras personas. Aumentando la ocupación de los vehículos, reduciría el número de vehículos en las calles.
- Tomar las rutas menos congestionadas. Quizás no más las cortas, pero las rutas con menos tráfico. Más movilidad, menos consumo, menos contaminación.
- Que permita a los semáforos coordinar sus tiempos y, así, reducir la congestión de un cruce. O, quizás, indicar en qué momento un vehículo va a cruzar una intersección y que de esta forma, el semáforo puede determinar el momento y duración de la luz verde.

## Agentes involucrados

- Coches: Los coches interactúan con otros agentes de su tipo simulando el tráfico que puede surgir en una ciudad, con base en ello pueden decidir entre esperar o tomar una ruta alternativa para llegar al destino que requieren. También son capaces de interactuar con los semáforos, y dependiendo del activo de este agente deciden entre frenar o seguir dentro de una intersección. Con los obstáculos interactúan similar como si fuera un coche, pero estos son estáticos.
- Semáforos: Estos agentes deciden si dejar el flujo de los agentes del tipo coche dentro de una celda, y exclusivamente interactúan con ellos. 
- Obstáculos: Los obstáculos son agentes estáticos, los cuales se fijan en cierta posición para impedir el paso de los agentes del tipo coche.
Destino: Otro agente estático que funge como objetivo de llegada para los agentes del tipo coche.  


## Link al documento final

[Proyecto Final](https://docs.google.com/document/d/1KUx-yMLLhpA7tczekrdXo6INNWtJhwlgHpVj2VaVSzg/edit?usp=sharing)

## Archivos

- Los archivos Finales se encuentran bajo las carpetas MesaModels (Modelo) y CarsGoBrr (Unity). Para inciar el modelo es necesario correr api.py en MesaModels.

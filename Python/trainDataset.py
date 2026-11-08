
import os
import json
import cv2 as cv
import mediapipe as mp
import numpy as np

from mediapipe.tasks import python
from mediapipe.tasks.python import vision


# ============================================================
# RUTAS
# ============================================================

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

dir_path = os.path.join(BASE_DIR, "Gestures")
output_path = os.path.join(BASE_DIR, "Drawing2")
json_path = os.path.join(BASE_DIR, "Models", "mx_landmarks.json")

os.makedirs(output_path, exist_ok=True)
os.makedirs(os.path.dirname(json_path), exist_ok=True)


# ============================================================
# MEDIAPIPE
# ============================================================

base_options = python.BaseOptions(
    model_asset_path=os.path.join(BASE_DIR, "hand_landmarker.task")
)

options = vision.HandLandmarkerOptions(
    base_options=base_options,
    num_hands=1,
    min_hand_detection_confidence=0.7,
    min_hand_presence_confidence=0.7,
    min_tracking_confidence=0.7
)

detector = vision.HandLandmarker.create_from_options(options)


# ============================================================
# DIBUJAR LANDMARKS
# ============================================================

mp_hands = mp.tasks.vision.HandLandmarksConnections
mp_drawing = mp.tasks.vision.drawing_utils
mp_drawing_styles = mp.tasks.vision.drawing_styles


def draw_landmarks_on_image(rgb_image, detection_result):

    annotated_image = np.copy(rgb_image)

    for hand_landmarks in detection_result.hand_landmarks:

        mp_drawing.draw_landmarks(
            annotated_image,
            hand_landmarks,
            mp_hands.HAND_CONNECTIONS,
            mp_drawing_styles.get_default_hand_landmarks_style(),
            mp_drawing_styles.get_default_hand_connections_style()
        )

    return annotated_image


# ============================================================
# DATASET
# ============================================================

mx_landmarks = []

total_procesadas = 0
total_descartadas = 0


# ============================================================
# RECORRER LEFT Y RIGHT
# ============================================================

for hand_folder in ["Left", "Right"]:

    hand_path = os.path.join(dir_path, hand_folder)

    if not os.path.isdir(hand_path):
        print(f"No existe la carpeta: {hand_path}")
        continue

    print(f"\nProcesando mano: {hand_folder}")

    for label in os.listdir(hand_path):

        folder_path = os.path.join(hand_path, label)

        if not os.path.isdir(folder_path):
            continue

        output_folder = os.path.join(output_path, hand_folder, label)
        os.makedirs(output_folder, exist_ok=True)

        for filename in os.listdir(folder_path):

            image_path = os.path.join(folder_path, filename)

            print(f"Procesando: {hand_folder}/{label}/{filename}")

            # ------------------------------------------------
            # CARGAR IMAGEN
            # ------------------------------------------------

            try:
                img = mp.Image.create_from_file(image_path)
            except Exception as e:
                print(f"No se pudo cargar {filename}: {e}")
                total_descartadas += 1
                continue

            result = detector.detect(img)

            # ------------------------------------------------
            # VERIFICAR DETECCIÓN
            # ------------------------------------------------

            if len(result.hand_landmarks) == 0:
                print("No se detectó ninguna mano.")
                total_descartadas += 1
                continue

            # Solo utilizamos una mano
            hand_landmarks = result.hand_landmarks[0]

            handedness = result.handedness[0][0].category_name

            # Comprobar que coincida con la carpeta
            if handedness != hand_folder:
                print(
                    f"ADVERTENCIA: carpeta {hand_folder}, "
                    f"MediaPipe detectó {handedness}. "
                    "Se descarta la imagen."
                )
                total_descartadas += 1
                continue

            # ------------------------------------------------
            # NORMALIZAR LANDMARKS
            # ------------------------------------------------

            x_base = hand_landmarks[0].x
            y_base = hand_landmarks[0].y

            current_landmarks = []

            for landmark in hand_landmarks:

                # Coordenadas relativas a la muñeca
                x = landmark.x - x_base
                y = landmark.y - y_base

                # Llevar ambas manos a una orientación común
                if handedness == "Left":
                    x = -x

                current_landmarks.extend([x, y])

            # Verificar las 42 características
            if len(current_landmarks) != 42:
                print("Cantidad incorrecta de landmarks.")
                total_descartadas += 1
                continue

            # Guardar etiqueta + 42 características
            fila = [label] + current_landmarks

            mx_landmarks.append(fila)

            total_procesadas += 1

            print("Características:", len(current_landmarks))

            # ------------------------------------------------
            # DIBUJAR LANDMARKS
            # ------------------------------------------------

            annotated_image = draw_landmarks_on_image(
                img.numpy_view(),
                result
            )

            # ------------------------------------------------
            # BOUNDING BOX
            # ------------------------------------------------

            height, width, _ = annotated_image.shape

            x_coordinates = [
                landmark.x for landmark in hand_landmarks
            ]

            y_coordinates = [
                landmark.y for landmark in hand_landmarks
            ]

            margin = 10

            box_x1 = max(0, int(min(x_coordinates) * width) - margin)
            box_y1 = max(0, int(min(y_coordinates) * height) - margin)

            box_x2 = min(width, int(max(x_coordinates) * width) + margin)
            box_y2 = min(height, int(max(y_coordinates) * height) + margin)

            imgCrop = annotated_image[box_y1:box_y2, box_x1:box_x2]

            if imgCrop.size == 0:
                print("Recorte vacío.")
                continue

            # ------------------------------------------------
            # REDIMENSIONAR Y CENTRAR
            # ------------------------------------------------

            imgSize = 300

            imgWhite = np.ones(
                (imgSize, imgSize, 3),
                np.uint8
            ) * 255

            h, w = imgCrop.shape[:2]

            aspectRatio = h / w

            if aspectRatio > 1:

                k = imgSize / h

                wCal = max(1, round(k * w))

                imgResize = cv.resize(
                    imgCrop,
                    (wCal, imgSize)
                )

                wGap = (imgSize - wCal) // 2

                imgWhite[
                    :, wGap:wGap + wCal
                ] = imgResize

            else:

                k = imgSize / w

                hCal = max(1, round(k * h))

                imgResize = cv.resize(
                    imgCrop,
                    (imgSize, hCal)
                )

                hGap = (imgSize - hCal) // 2

                imgWhite[
                    hGap:hGap + hCal, :
                ] = imgResize

            # Guardar imagen de visualización
            output_image_path = os.path.join(
                output_folder,
                f"Draw_{filename}"
            )

            cv.imwrite(
                output_image_path,
                cv.cvtColor(imgWhite, cv.COLOR_RGB2BGR)
            )


# ============================================================
# GUARDAR JSON UNA SOLA VEZ
# ============================================================

with open(json_path, "w", encoding="utf-8") as f:

    json.dump(
        mx_landmarks,
        f,
        indent=4
    )


# ============================================================
# RESULTADOS
# ============================================================

print("\n======================================")
print("DATASET TERMINADO")
print("======================================")

print("Total procesadas:", total_procesadas)
print("Total descartadas:", total_descartadas)
print("Características por muestra: 42")
print("JSON guardado en:", json_path)

detector.close()

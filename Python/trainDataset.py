import math
import cv2 as cv 
import mediapipe as mp 
import numpy as np 
import os
from mediapipe.tasks import python 
from mediapipe.tasks.python import vision 

mp_hands = mp.tasks.vision.HandLandmarksConnections 
mp_drawing = mp.tasks.vision.drawing_utils 
mp_drawing_styles = mp.tasks.vision.drawing_styles 
MARGIN = 10  # pixels 
FONT_SIZE = 1 
FONT_THICKNESS = 1 
HANDEDNESS_TEXT_COLOR = (88, 205, 54) # vibrant green 

base_options = python.BaseOptions(model_asset_path='Python/hand_landmarker.task') 
options = vision.HandLandmarkerOptions(base_options=base_options,num_hands=2, min_hand_detection_confidence = 0.7, 
                                       min_hand_presence_confidence=0.7, min_tracking_confidence=0.7 ) 
detector = vision.HandLandmarker.create_from_options(options) 



def draw_landmarks_on_image(rgb_image, detection_result): 
    hand_landmarks_list = detection_result.hand_landmarks 
    handedness_list = detection_result.handedness 
    annotated_image = np.copy(rgb_image) 

   
    # Loop through the detected hands to visualize. 
    for idx in range(len(hand_landmarks_list)): 
        hand_landmarks = hand_landmarks_list[idx] 
        handedness = handedness_list[idx] 

        # Draw the hand landmarks. 
        mp_drawing.draw_landmarks( 
        annotated_image, 
        hand_landmarks, 
        mp_hands.HAND_CONNECTIONS, 
        mp_drawing_styles.get_default_hand_landmarks_style(), 
        mp_drawing_styles.get_default_hand_connections_style()) 

        # Get the top left corner of the detected hand's bounding box. 
        height, width, _ = annotated_image.shape 
        x_coordinates = [landmark.x for landmark in hand_landmarks] 
        y_coordinates = [landmark.y for landmark in hand_landmarks] 
        text_x = int(min(x_coordinates) * width) 
        text_y = int(min(y_coordinates) * height) - MARGIN 

            # Draw handedness (left or right hand) on the image. 
        #cv.putText(annotated_image, f"{handedness[0].category_name}{handedness[0].score}", (text_x, text_y), cv.FONT_HERSHEY_DUPLEX, 
         #           FONT_SIZE, HANDEDNESS_TEXT_COLOR, FONT_THICKNESS, cv.LINE_AA) 

    return annotated_image 

dir_path = "Python/Natural Hand Digits Dataset"
output_path = "Python/Drawing"
os.makedirs(output_path, exist_ok=True)

labels = []
mx_landmarks = []

for label in os.listdir(dir_path):

    folder_path = os.path.join(dir_path, label)

    if os.path.isdir(folder_path):

        output_folder = os.path.join(output_path, label)
        os.makedirs(output_folder, exist_ok=True)

        for filename in os.listdir(folder_path):

            landmarks = []

            image_path = os.path.join(folder_path, filename)

            img = mp.Image.create_from_file(image_path)

            result = detector.detect(img) 

            annotated_image = draw_landmarks_on_image(img.numpy_view(), result) 

            output_image_path = os.path.join(output_folder, f"Draw_{filename}.jpg")
            #cv.imwrite(output_image_path, annotated_image)

            hand_landmarks_list = result.hand_landmarks
            handedness_list = result.handedness 

            for idx in range(len(hand_landmarks_list)): 
                hand_landmarks = hand_landmarks_list[idx] 
                handedness = handedness_list[idx] 

                for landmark in hand_landmarks_list[idx]:
                    landmarks.extend([landmark.x, landmark.y, landmark.z])

            fila = [label, handedness[0].category_name] + landmarks
            mx_landmarks.append(fila)

            #Dimensiones Bounding Box
            height, width, _ = annotated_image.shape 
            x_coordinates = [landmark.x for landmark in hand_landmarks] 
            y_coordinates = [landmark.y for landmark in hand_landmarks] 
            box_x1 = int(min(x_coordinates) * width) - MARGIN
            box_y1 = int(min(y_coordinates) * height) - MARGIN 
            box_x2 = int(max(x_coordinates) * width) + MARGIN
            box_y2 = int(max(y_coordinates) * height) + MARGIN 

            #CropImage
            imgSize = 300
            imgWhite = np.ones((imgSize, imgSize,3),np.uint8)*255
            imgCrop = annotated_image[box_y1:box_y2, box_x1:box_x2]

            h = (box_y2 - box_y1)
            w = (box_x2 - box_x1)
            aspectRatio = h/w

            if aspectRatio > 1:
                k = imgSize/h
                wCal = math.ceil(k*w) #Ancho calculado al escalar al tamño de blanco manteniendo la proporción

                wGap = math.ceil((300-wCal)/2)

                imgResize = cv.resize(imgCrop, (wCal, imgSize))
                imgWhite[0:imgResize.shape[0], wGap: wCal+wGap] = imgResize
            else:
                k = imgSize/w
                hCal = math.ceil(k*h) #Ancho calculado al escalar al tamño de blanco manteniendo la proporción

                hGap = math.ceil((300-hCal)/2)#para centrar

                imgResize = cv.resize(imgCrop, (imgSize, hCal))
                imgWhite[hGap:hCal+hGap, 0:imgResize.shape[1]] = imgResize


            cv.imwrite(output_image_path,  cv.cvtColor(imgWhite, cv.COLOR_RGB2BGR))
            
            print(f'imagen{filename} landmarks')

        print(mx_landmarks)

            #cv.imshow('Hand Detection', cv.cvtColor(annotated_image, cv.COLOR_RGB2BGR)) 

            #cv.imshow('Hand Detection', frame) 





 

 
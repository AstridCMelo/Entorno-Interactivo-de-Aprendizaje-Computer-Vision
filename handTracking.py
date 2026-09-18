import cv2 as cv 
import mediapipe as mp 
import numpy as np 
from mediapipe.tasks import python 
from mediapipe.tasks.python import vision 

cap = cv.VideoCapture(0) 
if not cap.isOpened(): 
    print("No se pudo abrir la webcam") 
    exit() 

cap.set(cv.CAP_PROP_FRAME_WIDTH, 800) 
cap.set(cv.CAP_PROP_FRAME_HEIGHT, 800) 

#mp_hands = mp.solutions.hands 
#mp_drawing = mp.solutions.drawing_utils 

base_options = python.BaseOptions(model_asset_path='hand_landmarker.task') 
options = vision.HandLandmarkerOptions(base_options=base_options,num_hands=2, min_hand_detection_confidence = 0.7, 
                                       min_hand_presence_confidence=0.7, min_tracking_confidence=0.7 ) 
detector = vision.HandLandmarker.create_from_options(options) 

#hand = mp_hands.Hands() 

mp_hands = mp.tasks.vision.HandLandmarksConnections 
mp_drawing = mp.tasks.vision.drawing_utils 
mp_drawing_styles = mp.tasks.vision.drawing_styles 
MARGIN = 10  # pixels 
FONT_SIZE = 1 
FONT_THICKNESS = 1 
HANDEDNESS_TEXT_COLOR = (88, 205, 54) # vibrant green 

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

        #if handedness[0].score > 0.85:
            # Draw handedness (left or right hand) on the image. 
        cv.putText(annotated_image, f"{handedness[0].category_name}{handedness[0].score}", (text_x, text_y), cv.FONT_HERSHEY_DUPLEX, 
                    FONT_SIZE, HANDEDNESS_TEXT_COLOR, FONT_THICKNESS, cv.LINE_AA) 

    return annotated_image 


while cap.isOpened(): 

    ret, frame = cap.read() 
    if not ret: 
        print("No se pudo capturar el frame") 
        break 

    frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB) 
    frameImage = mp.Image( image_format=mp.ImageFormat.SRGB, data=frame)
    result = detector.detect(frameImage) 
    #results = hand.process(frame) 
    frame = cv.cvtColor(frame, cv.COLOR_RGB2BGR) 

    #if results.multi_hand_landmarks: 
    #    for hand_landmarks in results.multi_hand_landmarks: 
    #        mp_drawing.draw_landmarks(frame, hand_landmarks, mp_hands.HAND_CONNECTIONS) 
    #        print(hand_landmarks) 

    annotated_image = draw_landmarks_on_image(frameImage.numpy_view(), result) 

    cv.imshow('Hand Detection', cv.cvtColor(annotated_image, cv.COLOR_RGB2BGR)) 

    #cv.imshow('Hand Detection', frame) 

    if cv.waitKey(1) & 0xFF == ord('q'): 

            break 

cap.release() 

cv.destroyAllWindows() 

 

 
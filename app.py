from flask import Flask, request, jsonify, send_file
import cv2
import numpy as np
import io

app = Flask(__name__)


processed_img = None

@app.route('/process-image', methods=['POST'])
def process_image():
    global processed_img
    file = request.files['image'].read()
    npimg = np.frombuffer(file, np.uint8)
    img = cv2.imdecode(npimg, cv2.IMREAD_COLOR)
    
    
    processed_img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

   
    return jsonify({"message": "Image processed successfully"})


@app.route('/get-processed-image', methods=['GET'])
def get_processed_image():
    global processed_img
    
   
    if processed_img is None:
        return jsonify({"error": "No processed image available"}), 404

    _, buffer = cv2.imencode('.png', processed_img)
    io_buf = io.BytesIO(buffer)
    return send_file(io_buf, mimetype='image/png')

if __name__ == "__main__":
    app.run(debug=True, host='0.0.0.0', port=5000)

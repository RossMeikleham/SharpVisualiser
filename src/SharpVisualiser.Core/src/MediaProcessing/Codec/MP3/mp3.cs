
//Obtains Info on MP3 Files 
public class MP3 {
    public int NFrames; // Number Of Frames in current MP3 
    private String fileName; // Name Of Current File 
    // fileStream
    
    MP3() {
    }

    MP3(String fileName) {
        //
    }

    // Get all frames in MP3
    public ArrayList<MP3Frame> Frames() {
        
    }

    // Optional/Maybe result
    public MP3Frame getFrame(int frameNo) {
        
    }

}

// Stores Info on Meta Data for MP3s 
public class MP3MetaData {
    String name;

}

// Each MP3 is made up of a number of frames
public class MP3Frame {
    static MP3Frame ParseMP3Frame(byte[] data) {
        
    }    
}

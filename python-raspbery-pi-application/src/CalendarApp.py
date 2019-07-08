# -*- coding: utf-8 -*-

# Form implementation generated from reading ui file 'test.ui'
#
# Created by: PyQt5 UI code generator 5.11.2
#
# WARNING! All changes made in this file will be lost!

from PyQt5 import QtCore, QtGui, QtWidgets
from googleapiclient.discovery import build
from functools import partial
from httplib2 import Http
from oauth2client import file, client, tools
import sys, datetime, threading
import json
import dateutil.parser
from pytz import timezone

class CalendarApp(object):

    def __init__(self, MainWindow, CalenderId, parent=None):

        SCOPES = 'https://www.googleapis.com/auth/calendar'
        self.CalenderId = CalenderId

        self.check_timezone()
        print("here")

        store = file.Storage('token.json')
        creds = store.get()
        if not creds or creds.invalid:
            flow = client.flow_from_clientsecrets('credentials.json', SCOPES)
            creds = tools.run_flow(flow, store)
        self.service = build('calendar', 'v3', http=creds.authorize(Http()))

        self.calendar = self.service.calendars().get(calendarId=CalenderId).execute()
        self.calendarID = CalenderId
        self.calendarList = self.get_calendar_list()

        MainWindow.setObjectName("MainWindow")
        MainWindow.resize(800, 600)
        self.time_diff = 0
        self.start_time = self.check_start_time()
        self.end_time = (self.start_time + datetime.timedelta(hours=0.5)).isoformat() + self.timezone
        self.time_not_iso = self.start_time
        self.start_time = self.start_time.isoformat() + self.timezone
        self.initUI(MainWindow)

        self.retranslate_ui(MainWindow)

        QtCore.QMetaObject.connectSlotsByName(MainWindow)

        self.show_event(self.start_time,  self.end_time)
        self.auto_time()
        self.get_upcoming_events()
       # self.add_event_list()

        try:
            timer = QtCore.QTimer(MainWindow)
            timer.timeout.connect(self.auto_time)
            # Runs the auto_time method every second
            timer.setInterval(1000)
            timer.start()
        
            update = QtCore.QTimer(MainWindow)
            update.timeout.connect(self.update_events)
            # Runs the update_events method every 20 seconds
            update.setInterval(20000)
            update.start()

            check_timezone = QtCore.QTimer(MainWindow)
            check_timezone.timeout.connect(self.check_timezone)
            # Runs the check_timezone method daily
            check_timezone.setInterval(86400000)
            check_timezone.start()

        except:
            print("failed timer")

    def check_timezone(self):
        is_dst = datetime.datetime.now(tz=timezone("Europe/London")).dst()
        if is_dst:
            self.timezone = '+01:00'
        else:
            self.timezone = 'Z'

    def get_calendar_list(self):
        building = json.loads(self.calendar['description'])
        temp_list = []
        page_token = None
        while True:
            temp_calendar_list = self.service.calendarList().list(pageToken=page_token).execute()
            for calendar_list_entry in temp_calendar_list['items']:
                try:

                    temp = json.loads(calendar_list_entry['description'])

                except:
                    print("")
                else:
                    if calendar_list_entry['summary'] != self.calendar['summary']:
                        if str(temp['building']) == str(building['building']):
                            temp_list.append(calendar_list_entry)

            page_token = temp_calendar_list.get('nextPageToken')
            if not page_token:
                break

        return temp_list

    def update_events(self):
        self.show_event(self.start_time, self.end_time)
        self.get_upcoming_events()

    def nearest_empty_room(self):

        text = ""
        # get a list of all calendars in the same building
        calendar_list = self.get_calendar_list()
        empty_room = []
        # checks to see whether or not the various rooms in the same building are empty or not
        for item in calendar_list:
            events = self.service.events().list(calendarId=item['id'], timeMin=self.start_time, timeMax=self.end_time).execute()
            if not events['items']:
                empty_room.append(item)

        room_num = int(self.calendar['summary'])
        i = 1

        # if there are any empty rooms loop through and increment each time one isn't found
        if empty_room:
            while text == "":
                for calendar in empty_room:
                    if room_num+i == int(calendar['summary']):
                        text = calendar['summary'] + "\t"
                        break
                    elif room_num-1 == int(calendar['summary']):
                        text = calendar['summary'] + "\t"
                        break
                i += 1

        if not empty_room:
            text = "No empty rooms nearby"

        # display nearest room to the user
        self.lbl_nearest.setText("  Nearest empty room:\n\n  " + text)
        font_size = QtGui.QFont("Times", 11, QtGui.QFont.Bold)
        self.lbl_nearest.setFont(font_size)

    def initUI(self, MainWindow):

        self.central_widget = QtWidgets.QWidget(MainWindow)
        self.central_widget.setAutoFillBackground(False)
        self.central_widget.setObjectName("central_widget")

        self.label = QtWidgets.QLabel(self.central_widget)
        self.label.setAutoFillBackground(True)
        self.label.setGeometry(QtCore.QRect(40, 50, 571, 371))
        self.label.setObjectName("label")
        self.label.setStyleSheet("""
        QLabel 
        { 
            border:1px solid rgb(255, 255, 255); 
            border-radius: 10px; 
            min-height: 20px; 
            min-width: 20px;
            color:#FDFFFC;}""")


        self.btn_prev_event = QtWidgets.QPushButton(self.central_widget)
        self.btn_prev_event.setGeometry(QtCore.QRect(140, 430, 101, 71))
        self.btn_prev_event.setObjectName("pushButton")
        self.btn_prev_event.clicked.connect(partial(self.current_event, 0))
        self.btn_prev_event.setDisabled(True)
        self.btn_prev_event.setStyleSheet("""
        QPushButton 
        { 
            border:1px solid rgb(255, 255, 255);
            border-radius: 10px; 
            min-height: 20px; 
            min-width: 20px; 
            color: white;
        }
        QPushButton:pressed
        {
            background-color: white;
            color: black;
        }
        QPushButton:disabled
        {
            color: grey;
            border:1px solid rgb(192, 192, 192);
        }
        """)

        self.btn_next_event = QtWidgets.QPushButton(self.central_widget)
        self.btn_next_event.setGeometry(QtCore.QRect(420, 430, 101, 71))
        self.btn_next_event.setObjectName("pushButton_2")
        self.btn_next_event.clicked.connect(partial(self.current_event, 1))
        self.btn_next_event.setStyleSheet("""
        QPushButton 
        { 
            border:1px solid rgb(255, 255, 255);
            border-radius: 10px; 
            min-height: 20px; 
            min-width: 20px; 
            color: white;
        }
        QPushButton:pressed
        {
            background-color: white;
            color: black;
        }""")


        self.btn_book = QtWidgets.QPushButton(self.central_widget)
        self.btn_book.setGeometry(QtCore.QRect(250, 430, 161, 71))
        self.btn_book.setObjectName("pushButton_3")
        self.btn_book.clicked.connect(self.book_event)
        self.btn_book.setStyleSheet("""
        QPushButton 
        { 
            border:1px solid rgb(255, 255, 255);
            border-radius: 10px; 
            min-height: 20px; 
            min-width: 20px; 
            color: white;
        }
        QPushButton:pressed
        {
            background-color: white;
            color: black;
        }""")

        self.btn_nearest_empty = QtWidgets.QPushButton(self.central_widget)
        self.btn_nearest_empty.setGeometry(QtCore.QRect(250, 430, 161, 71))
        self.btn_nearest_empty.clicked.connect(self.nearest_empty_room)
        self.btn_nearest_empty.setStyleSheet("""
        QPushButton 
        { 
            border:1px solid rgb(255, 255, 255);
            border-radius: 10px; 
            min-height: 20px; 
            min-width: 20px; 
            color: white;
        }
        QPushButton:pressed
        {
            background-color: white;
            color: black;
        }""")

        self.btn_nearest_empty.hide()
        self.lbl_room = QtWidgets.QLabel(self.central_widget)
        self.lbl_room.setGeometry(QtCore.QRect(310, 10, 100, 30))
        self.lbl_room.setStyleSheet("""
        QLabel
        { 
            color: white;
        }""")

        self.lbl_time = QtWidgets.QLabel(self.central_widget)
        self.lbl_time.setGeometry(QtCore.QRect(40, 430, 90, 71))
        self.lbl_time.setObjectName("lbl_time")
        self.lbl_time.setStyleSheet("""
        QLabel
        { 
            border:1px solid rgb(255, 255, 255);
            border-radius: 10px; 
            min-height: 20px; 
            min-width: 20px; 
            color: white;
        }""")

        self.lbl_nearest = QtWidgets.QLabel(self.central_widget)
        self.lbl_nearest.setGeometry(QtCore.QRect(530, 430, 220, 71))
        self.lbl_nearest.setStyleSheet("""
        QLabel
        { 
            border:1px solid rgb(255, 255, 255);
            border-radius: 10px; 
            min-height: 20px; 
            min-width: 20px; 
            color: white;
        }""")



        self.lbl_upcoming = QtWidgets.QLabel(self.central_widget)
        self.lbl_upcoming.setGeometry(QtCore.QRect(620, 50, 130, 370))
        self.lbl_upcoming.setObjectName("lbl_upcoming")
        self.lbl_upcoming.setStyleSheet("""
        QLabel 
        { 
            
            border:1px solid rgb(255, 255, 255);
            border-radius: 10px; 
            min-height: 20px; 
            min-width: 20px; 
            color: white;
        }""")

        self.lbl_upcoming.setAlignment(QtCore.Qt.AlignTop)


        self.central_widget.setStyleSheet("background-color:#1E1E1E")
        MainWindow.setCentralWidget(self.central_widget)
        self.menubar = QtWidgets.QMenuBar(MainWindow)
        self.menubar.setGeometry(QtCore.QRect(0, 0, 800, 21))
        self.menubar.setObjectName("menubar")

        MainWindow.setMenuBar(self.menubar)

    def on_list_selected(self, index):
        print('self.model.data(index).toString(): %s' % self.listView.model)

    def background_event_list(self):
        thread = threading.Thread(target=self.add_event_list())
        thread.start()

    def book_event(self):
        summary = 'Ad Hoc Meeting '
        event = {
            'summary': summary,
            'start': {
                'dateTime': self.start_time,
                'timeZone': 'Europe/London',
            },
            'end': {
                'dateTime': self.end_time,
                'timeZone': 'Europe/London',
            },
            'extendedProperties': {
                'shared': {
                    'owner': "ad hoc",
                }
            }
        }
        try:
            event = self.service.events().insert(calendarId=self.CalenderId,
                                                 body=event).execute()
        except:
            print("failed to add event")
        else:
            print('Event created: %s' % (event.get('htmlLink')))
            self.show_event(self.start_time, self.end_time)
            #self.add_event_list()


    def get_upcoming_events(self):
        now = self.check_start_time() + datetime.timedelta(hours=0.5)

        now = now.isoformat() + self.timezone

        events = self.service.events().list(calendarId=self.CalenderId, timeMin=now,singleEvents=True, maxResults=5).execute()

        text = " Upcoming\n\n"
        for event in events['items']:
            text += event['summary'] + "\n" + self.get_event_times(event) + "\n\n"
        self.lbl_upcoming.setText(text)

    def retranslate_ui(self, MainWindow):
        _translate = QtCore.QCoreApplication.translate
        MainWindow.setWindowTitle(_translate("MainWindow", "MainWindow"))
        self.label.setText(_translate("MainWindow", "TextLabel"))
        self.label.setAlignment(QtCore.Qt.AlignCenter)
        fontsize = QtGui.QFont("Times", 20, QtGui.QFont.Bold)
        self.label.setFont(fontsize)

        text = "Room " + self.calendar['summary']
        self.lbl_room.setText(_translate("Mainwindow", text))

        self.btn_prev_event.setText(_translate("MainWindow", "Previous"))
        self.btn_next_event.setText(_translate("MainWindow", "Next"))
        self.btn_book.setText(_translate("MainWindow", "Book"))
        self.btn_nearest_empty.setText(_translate("MainWindow", "Nearest available room"))
        self.lbl_time.setText(_translate("MainWindow", "TextLabel"))
        self.lbl_upcoming.setText(_translate("MainWindow", "Upcoming Bookings"))

    def show_event(self, time_min, time_max):
        time = self.time_not_iso.strftime("%H:%M") + " - " + (self.time_not_iso + datetime.timedelta(hours=0.5)).strftime("%H:%M")
        # look for an event within the specified time frame
        events = self.service.events().list(calendarId=self.CalenderId, timeMin=time_min, timeMax=time_max).execute()
        # if there isn't an event
        if not events['items']:
            text = "Free\n" + time
            self.label.setText(text)
            # set colour to green
            self.label.setStyleSheet(
                "QLabel { border:1px solid rgb(255, 255, 255);background-color:#4D8E1B; border-radius: 10px; min-height: 20px; min-width: 20px;color:#FDFFFC;}")
            self.btn_book.show()
            self.btn_nearest_empty.hide()
            self.lbl_nearest.setText("Room is free")
        # if there is an event
        for event in events['items']:
            # get the events start and end time
            time = self.get_event_times(event)
            try:
                text = "Owner: " + event['extendedProperties']['shared']['owner'] + "\n"
            except:
                text = "Owner: \n"
            try:
                text += event['summary'] + "\n" + time
            except:
                text += "\n" + time
            self.label.setText(text)
            # set colour to red
            self.label.setStyleSheet(
                "QLabel { border:1px solid rgb(255, 255, 255);background-color:#d10c0c;; border-radius: 10px; min-height: 20px; min-width: 20px; color:#FDFFFC;}")
            self.btn_book.hide()
            self.btn_nearest_empty.show()

    # d10c0c

    def get_event_times(self, event):
        #time = datetime.datetime.strptime(event['start'].get('dateTime'), "%Y-%m-%dT%H:%M:%S%z").time().strftime("%H:%M") + " - " + datetime.datetime.strptime(event['end'].get('dateTime'), "%Y-%m-%dT%H:%M:%S%z").time().strftime("%H:%M")
        time = dateutil.parser.parse(event['start'].get('dateTime')).strftime("%H:%M")  + " - " +   dateutil.parser.parse(event['end'].get('dateTime')).strftime("%H:%M")
        return time

    def auto_time(self):
        _translate = QtCore.QCoreApplication.translate
        # Shows the current time and date in the format 09:50:00 monday January 7
        time = datetime.datetime.now().strftime(" %H:%M:%S \n %A %B %d")
        self.lbl_time.setText(_translate("MainWindow", time))

    def current_event(self, i):
        # Depending on which button is pressed, increments or decrements the variable
        print(i)
        if i == 1:
            self.time_diff += 0.5
        else:
            self.time_diff -= 0.5

        self.start_time = self.check_start_time()

        if self.time_diff == 0:

            # set the start and end time to the required iso format
            self.end_time = (self.start_time + datetime.timedelta(
                hours=0.5)).isoformat() + self.timezone
            self.time_not_iso = self.start_time
            self.start_time = self.start_time.isoformat() + self.timezone
            self.btn_prev_event.setDisabled(True)

        else:

            # set the start and end time to the required iso format
            self.start_time = (self.start_time + datetime.timedelta(hours=self.time_diff))
            self.time_not_iso = self.start_time
            self.end_time = (self.start_time + datetime.timedelta(hours=0.5)).isoformat() + self.timezone
            self.start_time = self.start_time.isoformat() + self.timezone
            self.btn_prev_event.setDisabled(False)

        print(self.start_time, self.time_diff)
        print(self.end_time)
        self.show_event(self.start_time, self.end_time)

    def check_start_time(self):
        if datetime.datetime.now().minute > 30:
            temp = datetime.datetime.now().replace(microsecond=0, second=0, minute=30)
        else:
            temp = datetime.datetime.now().replace(microsecond=0, second=0, minute=0)

        return temp

